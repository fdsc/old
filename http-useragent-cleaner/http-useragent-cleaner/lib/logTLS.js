const {Cc, Ci, Cu, Cr, Cm, components} = require("chrome");
var pipnss  = Cc['@mozilla.org/intl/stringbundle;1'].getService(Ci.nsIStringBundleService).createBundle('chrome://pipnss/locale/pipnss.properties');

exports.logger = function()
{
	this.tabs = [];
};

exports.logger.prototype =
{
	urls: require('./getURL'),
	pipnss: pipnss,
	CertDb: Cc['@mozilla.org/security/x509certdb;1'].getService(Ci.nsIX509CertDB),
/*
	objectToConsole: function(obj, n, prefix, filter, keys)
	{
		if (n <= 0)
			return;

		var nm = Object.keys(obj);
		for (var a of nm)
			try
			{
				var al = a.toLowerCase();
				var val = obj[a];
				if (!filter || al.indexOf(filter) >= 0)
					if (!(val instanceof Function) && (!keys || keys.indexOf(al) < 0))
						console.error(prefix + a + ': ' + val);

				if (!(val.toLocaleUpperCase instanceof Function) && (!keys || keys.indexOf(al) < 0))
					this.objectToConsole(val, n - 1, prefix + a + '.', filter, keys);
			}
			catch (e)
			{}
	},
*/
	getSecurityInfo: function(httpChannel)
	{
		var si = httpChannel.securityInfo;

		if (si instanceof Ci.nsISSLStatusProvider)
		{
			return si.QueryInterface(Ci.nsISSLStatusProvider);
		}
		
		return null;
	},

	// http://oid-info.com
	getHashL: function(encryptionString, algName)
	{
		// коэффициент ослабления криптостойкости sha-1
		// используется и для sha-2, т.к. алгоритмы похожи
		var cl = 57/80;
		if (encryptionString.indexOf('SHA-512') > 0 || encryptionString.indexOf('SHA512') > 0)
		{
			return [512, 512*cl, 'sha-2 512'];
		}
		
		if (encryptionString.indexOf('SHA-384') > 0 || encryptionString.indexOf('SHA384') > 0)
		{
			return [384, 384*cl, 'sha-2 384'];
		}
		
		if (encryptionString.indexOf('SHA-256') > 0 || encryptionString.indexOf('SHA256') > 0)
		{
			return [256, 256*cl, 'sha-2 256'];
		}
		
		if (encryptionString.indexOf('SHA-224') > 0 || encryptionString.indexOf('SHA224') > 0)
		{
			return [224, 224*cl, 'sha-2 224'];
		}
		
		if (encryptionString.indexOf('SHA-1') > 0 || encryptionString.indexOf('SHA1') > 0)
		{
			return [160, 160*cl, 'sha-1 (160)'];
		}

		if (!algName && encryptionString.indexOf('SHA') == encryptionString.length - 3 && encryptionString.length > 9)
		{
			return [160, 160*cl, 'sha-1 (160)'];
		}
		
		if (encryptionString.indexOf('MD5') > 0)
		{
			return [128, 128*39/64, 'MD5 (128)'];
		}
		
		if (encryptionString.indexOf('MD2') > 0)
		{
			return [128, 128*63.3/128, 'MD2 (128)'];
		}

		// http://www.oid-info.com
		// CertDumpDefOID
		// Object Identifier (%S)
		// Ќапример, (1 2 840 10045 4 3 2)

		var OIString = pipnss.GetStringFromName('CertDumpDefOID');
		    OIString = OIString.substring(0, OIString.length - 3);

		if (encryptionString.indexOf(OIString) == 0)
		{
			if (encryptionString.indexOf('(1 2 840 10045 4') >= 0)
			{
				// ”бираем начало и концевую круглую скобку
				// substr(OIString.length, encryptionString.length - OIString.length - 1);
				var result = encryptionString.substring(OIString.length, encryptionString.length - 1);
				result = result.split(' ');
				result.splice(0, 5);

				if (result[0] == '1')
					return [160, 160*cl, 'sha-1 (160)'];

				if (result[0] == '3')
				{
					if (result[1] == '1')
						return [224, 224*cl, 'sha-2 224'];
					if (result[1] == '2')
						return [256, 256*cl, 'sha-2 256'];
					if (result[1] == '3')
						return [384, 384*cl, 'sha-2 384'];
					if (result[1] == '4')
						return [512, 512*cl, 'sha-2 512'];
				}
			}
		}

		console.error('HUAC ERROR: certificate type unknown; please send information to HUAC developer with url and 2 lines below');
		console.error(OIString);
		console.error(encryptionString);

		return [0, 0, 'unknown'];
	},

	getR: function(x1, y1, x2, y2, x)
	{
		return ((x - x1)*y2 + (x2 - x)*y1) / (x2 - x1);
	},

	getP: function(bits, base)
	{
		// Считаем, что запас от возможностей крупной ОПГ до возможностей мелкой ОПГ составляет 9 битов
		// Между шпаной и мелкой ОПГ 3 бита
		if (bits-base <= -64)
			return 0.00001;

		if (bits >= 256)
			return 0.999999;

		var b = 0;

		// Если запас более 15-ти битов, то идёт вычисление вероятности между 0,999999 и 0,95
		if (bits-base > 64)
			b = this.getR(64, 0.985, 256-base, 0.999999, bits-base);
		else
		if (bits-base > 15)
			b = this.getR(15, 0.90, 64, 0.985, bits-base);
		else
		// Если запаса нет, то между 0,10 и 0,95
		if (bits-base >= 0)
			b = this.getR(0, 0.20, 15, 0.90, bits-base);
		else
		if (bits-base >= -9)
			b = this.getR(-9, 0.10, 0, 0.20, bits-base);
		else
		if (bits-base >= -12)
		{
			// Это - когда совсем всё плохо, чтоб не давать ноль тем, кто совсем опаздывает
			b = this.getR(-12, 0.08, -9, 0.10, bits-base);
		}
		else
		if (bits-base >= -28)
		{
			// Это - когда совсем всё плохо, чтоб не давать ноль тем, кто совсем опаздывает
			b = this.getR(-28, 0.04, -12, 0.08, bits-base);
		}
		else
		{
			// Это - когда совсем всё плохо, чтоб не давать ноль тем, кто совсем опаздывает
			b = this.getR(-64, 0.00001, -28, 0.04, bits-base);
		}

		if (isNaN(b))
			b = 0.00001;

		if (b > 0.999999)
			b = 0.999999;
		else
		if (b < 0.00001)
			b = 0.00001;

		return b;
	},
	
	baseCP: 511,
	
	getEqRSA: function(bits)
	{
		if (bits < 2048)
		{
			return this.getR(947, 70, 2048, 102, bits);
		}
		else
		// Ёквивалент 124
		if (bits < 3072)
		{
			return this.getR(2048, 102, 3072, 124, bits);
		}
		else
		// Ёквивалент 142
		if (bits < 4096)
		{
			return this.getR(3072, 124, 4096, 142, bits);
		}
		else
		// Ёквивалент 194
		if (bits < 8192)
		{
			return this.getR(4096, 142, 8192, 194, bits);
		}
		else
			return this.getR(8192, 194, 14596, 250, bits);
	},
	
	getBitsToCorrect: function(issueDate)
	{
		// Убывание происходит по экспоненте
		// 1/1,5*2^-y/1.5 (это за год)
		// Интеграл int(sym('2^(-x/1.5)'), 0, x)
		// 2.164 - 2.164/exp(0.462*x)
		return (2.164 - 2.164/Math.exp(0.462*issueDate));
	},

	getCertF: function(certificates, crt, TLSObjectAddA, PFS, isFirst, lowCertsCount, certsHierarchyCount)
	{
		var rootCert = !crt.issuer;

		var result = {h: {f: 1.0, flong: 1.0, msg: []}, s: {f: 1.0, flong: 1.0, msg: []}, st: {f: 1.0, flong: 1.0, msg: []}};
		var TLSObjectAddH = function(f, flong, msg, value, noK, noK10)
		{
			TLSObjectAddA(result.h, f, flong, msg, value, noK, noK10);
		};
		var TLSObjectAddS = function(f, flong, msg, value, noK, noK10)
		{
			TLSObjectAddA(result.s, f, flong, msg, value, noK, noK10);
		};
		var TLSObjectAddST = function(f, flong, msg, value, noK, noK10)
		{
			TLSObjectAddA(result.st, f, flong, msg, value, noK, noK10);
		};

		signatureAlg = this.getSignatureAlgNameFromCertASN1(certificates);

		result.signatureAlg = signatureAlg;

		var BitsToCorrect = 0;
		var issueDate = Date.now() - crt.validity.notBefore/1000;
		var issueDateStr = new Date(crt.validity.notBefore/1000).toLocaleDateString();
		issueDate     = issueDate / (1000*60*60*24*365.25);
		// Cчитаем, что алгоритмы те же, но при этом удвоение происходит раз в 1.5 года
		BitsToCorrect = this.getBitsToCorrect(issueDate); //Math.log(issueDate)/Math.log(1.5);

		if (BitsToCorrect < 0)
		{
			console.error('HUAC: error in logTLS.getCertF - no cert issue date ' + BitsToCorrect + ' ' + crt.validity.notBefore);
			BitsToCorrect = 80;
		}

		/*}
		else
			console.error('HUAC: error in logTLS.getCertF - no cert issue date');
		*/

		var baseAgeP  = isFirst ? 0.92 : 0.962;	// Вероятность не похищения за год
		if (lowCertsCount == certsHierarchyCount - 1 && certsHierarchyCount > 2)
			baseAgeP = 0.98;	// для корневого сертификата

		if (PFS)
		{
			var baseAgePO = 0.8;	// Вероятность необнаружения за год
			// var powID = Math.pow(baseAgeP, issueDate);
			var powID = 1.0;
			var iDate = issueDate;

			// Чтобы просто так оценку не портить, корректируем её
			// Когда есть HPKP, искользование сертификатов менее, чем на месяц вообще очень неудобно, чего там.
			if (isFirst)
				iDate -= 1/12;
			else
			// Промежуточный сертификат в полгода считается полностью безопасным
			// lowCertsCount == 0 - сертификат сервера
			// lowCertsCount == certsHierarchyCount - 1 - корневой сертификат
			if (lowCertsCount > 0 && lowCertsCount < certsHierarchyCount - 1)
				iDate -= 0.5;
			else
			// Корневой сертификат на 1,5 года считается полностью безопасным
				iDate -= 1.5;

			while (iDate > 0)
			{
				if (iDate >= 1.0)
					powID *= 1.0 - (1.0 - baseAgeP) * Math.pow(baseAgePO, iDate);
				else
					powID *= 1.0 - (1.0 - Math.pow(baseAgeP, iDate)) * Math.pow(baseAgePO, iDate);

				iDate -= 1.0;
			}
			TLSObjectAddST(powID, powID, 'Age (steal)', '' + Math.floor(issueDate*10)/10, true, true);
		}
		else
		{
			var powID = Math.pow(baseAgeP, issueDate);

			TLSObjectAddST(powID, Math.pow(baseAgeP, (crt.validity.notAfter - crt.validity.notBefore)/1000/(1000*60*60*24*365.25)), 'Age (steal)', '' + Math.floor(issueDate*100)/100, 1, true);
		}

		// http://www.keylength.com/en/7/ RFC3766
		// Рассчёт fl проводитс€ с запасом на 50 лет
		// Рассчёт производитс€ из того, что в 80-ом году должно было быть закончено применение 56-битного ключа (см. выше, реально слабее)
		// ƒалее, 1 бит каждые 1.5 года
		// D:\Arcs\Internet\browsers\develop\http-useragent-cleaner\test\cryptGraph
		if (signatureAlg[0] == 'RSA')
		{
			TLSObjectAddS(this.getEqRSA(signatureAlg[1])-BitsToCorrect, 0.001, 'Signature', 'RSA ' + signatureAlg[1] + ' (' + issueDateStr + ')', false, PFS);
		}
		else
		if (signatureAlg[0] == 'ECC')
		{
			// Ёквивалент 135
			if (signatureAlg[1] < 256)
			{
				TLSObjectAddS(this.getR(0, 0, 256, 135, signatureAlg[1])-BitsToCorrect, 0.1, 'Signature', 'Elleptic curves ' + signatureAlg[1] + ' (' + issueDateStr + ')', false, PFS);
			}
			else
			// Ёквивалент 200
			if (signatureAlg[1] < 384)
			{
				TLSObjectAddS(this.getR(256, 135, 384, 200, signatureAlg[1])-BitsToCorrect, 0.0, 'Signature', 'Elleptic curves ' + signatureAlg[1] + ' (' + issueDateStr + ')', false, PFS);
			}
			else
			// Ёквивалент 265
			if (signatureAlg[1] < 512)
			{
				TLSObjectAddS(this.getR(384, 200, 512, 265, signatureAlg[1])-BitsToCorrect, 0.99, 'Signature', 'Elleptic curves ' + signatureAlg[1] + ' (' + issueDateStr + ')', false, PFS);
			}
			else
				TLSObjectAddS(265, false, 'Signature', 'Elleptic curves ' + signatureAlg[1] + ' (' + issueDateStr + ')');
		}
		else
		{
			console.error("Http UserAgent Cleaner: unknown signatuire algoritm " + signatureAlg[0] + "/" + signatureAlg[1]);

			var notifications = require("sdk/notifications");
			notifications.notify
			({
				title: 		'Http UserAgent Cleaner',
				text: 		'unknown signatuire algoritm',
				iconURL: 	HTTPUACleaner['sdk/self'].data.url("HTTPUACleaner_trustlevel.png")
			});
			
			result.f *= 0.1;
			result.flong *= 0.1;

			TLSObjectAddS(0.1, 0.1, 'Signature', 'unknown ' + signatureAlg[0] + '/' + signatureAlg[1], true, true);
		}


		 // 4 / certificates.rowCount-2
		var encryptionString = certificates.getDisplayData(4).replace(/PKCS #1/g, '').replace(/Encryption/g, '');

		var hashLA = this.getHashL(encryptionString, signatureAlg[0]);
		var hashL  = hashLA[1];

		if (hashL == 0)
		{
			console.error("Http UserAgent Cleaner: unknown certificate hash algoritm " + signatureAlg[0] + "/" + signatureAlg[1] + ' / ' + encryptionString);
			
			var notifications = require("sdk/notifications");
			notifications.notify
			({
				title: 		'Http UserAgent Cleaner',
				text: 		'unknown certificate hash algoritm: ' + encryptionString,
				iconURL: 	HTTPUACleaner['sdk/self'].data.url("HTTPUACleaner_trustlevel.png")
			});

			result.f *= 0.1;
			result.flong *= 0.1;
			
			TLSObjectAdd(0.1, 0.1, 'Signature hash', 'unknown', true, true);
		}
		else
		{
			var hashLelevs = [
					{a: 160, f: 0.001, l: 0.001},
					{a: 224, f: 112,  l: 0.001},	// за счЄт того, что некоторые сертификаты имеют слишком большие сроки
					{a: 256, f: 128, l: 0.0},
					{a: 384, f: 192, l: 0.0},
					{a: 512, f: 256, l: 0.0},
					{a: 0, f: 1.0, l: 1.0}
					];
/*
			for (var i = 0; i < hashLelevs.length; i++)
			{
				var cl = hashLelevs[i];
				if (cl.a == 0)
				{
					TLSObjectAdd(false, false, 'Signature hash', hashL, false, true);
					break;
				}
				else
				if (cl.a > hashL)
				{
					TLSObjectAdd(hashL/2, cl.l, 'Signature hash', hashL, false, true);
					break;
				}
			}*/
			if (hashL >= 512 || rootCert)
				TLSObjectAddH(false, false, rootCert ? 'Signature hash (not use because root)' : 'Signature hash', hashLA[2], false, true);
			else
				TLSObjectAddH(hashL/2 - BitsToCorrect, 0.0, 'Signature hash', hashLA[2], false, true);
		}
		
		result.hashL = hashL;

		return result;
	},
	
	getTLSInfo: function(nsISSLStatusProviderObject, certsHostsTracking, isHPKP, base64, httpChannel, fxVersion51, hpkpdigest)
	{
		/*
			console.error(si.QueryInterface(Ci.nsISSLStatusProvider).SSLStatus);
			console.error(si.QueryInterface(Ci.nsISSLStatusProvider).SSLStatus.cipherName);	// cipherSuite
			console.error(si.QueryInterface(Ci.nsISSLStatusProvider).SSLStatus.secretKeyLength);	// symmetric
			*/
			// isUntrusted:false
			// isExtendedValidation:false
			// isNotValidAtThisTime:false
			// isDomainMismatch:false

			// serialNumber:"0A:1E:72:5D:CA:DE:8F:93:99:A9:37:DE:26:96:8C:86"
			// sha1Fingerprint:"47:D9:C3:EB:F4:01:84:C0:B5:F1:48:61:02:3C:AF:AB:07:1F:7A:C5"
			// sha256Fingerprint:"67:C4:22:1F:74:15:62:01:93:44:DD:7A:FE:C4:C9:62:1E:66:55:D1:C1:16:1F:08:E7:D5:A8:92:C8:BD:DF:13"
			// sha256SubjectPublicKeyInfoDigest:"8lt8dSIR4IampbVd5RCvWSYLDqAfJjGrdjVo3JxesW4="
			// subjectName:"CN=*.cdn.mozilla.net,OU=Technology,O=Mozilla Corporation,L=Mountain View,ST=California,C=US"
			// windowTitle:"*.cdn.mozilla.net"
			// nickname:"(нет псевдонима)"
			// organization:"Mozilla Corporation"
			// organizationalUnit:"Technology"
			// emailAddress:"(нет адреса)"
			// commonName:"*.cdn.mozilla.net"
			// certType:0	// не уверен, что он правильный

			// issuer:XPCWrappedNative_NoHelper
			// issuerCommonName:"DigiCert High Assurance CA-3"
			// issuerName:"CN=DigiCert High Assurance CA-3,OU=www.digicert.com,O=DigiCert Inc,C=US"
			// issuerOrganization:"DigiCert Inc"
			// issuerOrganizationUnit:"www.digicert.com"
			// isSelfSigned:false

			if (!(nsISSLStatusProviderObject instanceof Ci.nsISSLStatusProvider))
				return null;

			// http://www.enisa.europa.eu/activities/identity-and-trust/library/deliverables/algorithms-key-sizes-and-parameters-report
			// http://www.keylength.com/en/7/
			//  оэффициент на 1 июл€ 2015 года
			// Копия этого кода в mainTable
			var dateX    = (Date.now() - new Date('07/01/2015').getTime())/(1000*60*60*24*365.25);
			var dateX10  = dateX + 50.0;
			var kDate    = 0;//Math.pow(1/2, dateX  /1.5);
			var kDate10  = 0;//Math.pow(1/2, dateX10/1.5);

			this.baseCP  = 74 + dateX/1.5;
			var baseCP   = this.baseCP;
			var getP     = this.getP.bind(this);

			var TLSObject = {state: '', f: 0.0, flong: 0.0, fHPKP: 0.0, msg: []};
			var TLSObjectAddA = function(object, f, flong, msg, value, noK, noK10, fHPKP)
			{
				var kFunc = function(k, kDate)
				{/*
					var km = (1.0 - k)/kDate;
					km = 1.0 - km;

					// http-useragent-cleaner\test\cryptGraph
					if (km >= 0.0001)
						return km;
					
					return 0.0001;*/

					return getP(k, kDate);
				};


				var fa = f;
				var fl = flong;
				if (f !== false && f != 1.0)
				{
					fa        = noK ? f : kFunc(f, baseCP);

					if (!fHPKP)
						object.f *= fa;
					else
						object.fHPKP *= fa;
				}
				else
				{
					fa = 1.0;
				}

				if (flong !== false && flong != 1.0)
				{
					// ƒл€ ручного управлени€ неизмен€емым коэффициентом длительной оценки
					if (noK === 1)
					{
						fl = flong;
					}
					else
					{
						fl = noK10 ? fa : kFunc(f, baseCP + 50/1.5);
					}

					if (!fHPKP)
						object.flong *= fl;
				}
				else
					fl = 1.0;

				var middleColor = fHPKP ? 0.5 : 0.8;
				if (msg == 'Signature and signature hash')
					middleColor = 0.6;

				object.msg.push({f: fa, flong: fl, fo: object.f, flo: object.flong, msg: msg, value: value, middleColor: middleColor, fHPKP: !!fHPKP});
			};
			
			var TLSObjectAdd = function(f, flong, msg, value, noK, noK10, fHPKP)
			{
				TLSObjectAddA(TLSObject, f, flong, msg, value, noK, noK10, fHPKP);
			};

			var I = nsISSLStatusProviderObject;

			var TLSState = I.SSLStatus;
			if (!TLSState)
			{
				TLSObject.state = 'unknown';
				return TLSObject;
			}

			TLSState.QueryInterface(Ci.nsISSLStatus);

			TLSObject.f = 1.0;
			TLSObject.flong = 1.0;
			TLSObject.fHPKP = 0.0;

			if (TLSState.isUntrusted || TLSState.isNotValidAtThisTime || TLSState.isDomainMismatch)
			{
				TLSObject.state = 'untrusted';
				if (TLSState.isDomainMismatch)
				{
					TLSObject.state += '/DomainMismatch';
					TLSObjectAdd(0.001, 0.001, 'Domain mismatch', false, true, true);
				}
				else
				if (TLSState.isUntrusted)
				{
					TLSObject.state += '/untrusted';
					TLSObjectAdd(0.001, 0.001, 'Trusted', false, true, true);
				}
				else
				if (TLSState.isNotValidAtThisTime)
				{
					TLSObject.state += '/TimeExpired';
					TLSObject.f = 0.001;
					TLSObject.flong = 0.001;
				}
			}
			else
			{
				TLSObject.state = 'trusted';
			}

			TLSObject.ExtendedValidation = TLSState.isExtendedValidation;
			if (TLSObject.ExtendedValidation)
			{
				TLSObject.state += '/ExtendedValidation';
				TLSObjectAdd(false, false, 'ExtendedValidation', true, true, true);
			}
			else
			{
				TLSObjectAdd(0.999, 0.999, 'ExtendedValidation', false, true, true);
			}

			let ctMsg = 'Certificate transparency';
			TLSObject.certificateTransparency = TLSState.certificateTransparencyStatus;

			if (fxVersion51 < 0)
			{
				TLSObjectAdd(false, false, ctMsg, 'Not implemented in FireFox 50', true, true);
			}
			else
			if (TLSObject.certificateTransparency == Ci.nsISSLStatus.CERTIFICATE_TRANSPARENCY_OK)
			{
				if (fxVersion51 >= 0)
				{
					TLSObject.state += '/CertificateTransparency';
					TLSObjectAdd(false, false, ctMsg, true, true, true);
				}
				else
				{
					TLSObjectAdd(false, false, ctMsg, 'Not implemented in FireFox 50', true, true);
				}
			}
			else
			{
				switch (TLSObject.certificateTransparency)
				{
					case Ci.nsISSLStatus.CERTIFICATE_TRANSPARENCY_NONE:

						let now = Date.now();
						if (TLSObject.ExtendedValidation)
						{
							
							if (now < 1507593600000)
							{
								let ctr = this.getR(1507593600000, 0.2, 1483228800000, 0.990, now);
								TLSObjectAdd(ctr, ctr, ctMsg, false, true, true);
							}
							else
								TLSObjectAdd(0.2, 0.2, ctMsg, false, true, true);
						}
						else
						{
							if (now < 1546300800000)
							{
								let ctr = this.getR(1546300800000, 0.8, 1483228800000, 0.995, now);
								TLSObjectAdd(ctr, ctr, ctMsg, false, true, true);
							}
							else
								TLSObjectAdd(0.8, 0.8, ctMsg, false, true, true);
						}

						break;
					case Ci.nsISSLStatus.CERTIFICATE_TRANSPARENCY_NOT_APPLICABLE:
						TLSObjectAdd(false, false, ctMsg, 'Check not performed', true, true);
						break;
					case Ci.nsISSLStatus.CERTIFICATE_TRANSPARENCY_UNKNOWN_LOG:
						TLSObjectAdd(0.10, 0.10, ctMsg, 'Unknown log', true, true);
						break;
					case Ci.nsISSLStatus.CERTIFICATE_TRANSPARENCY_INVALID:
						TLSObjectAdd(0.05, 0.05, ctMsg, 'INVALID', true, true);
						break;
					default:
						TLSObjectAdd(0.01, 0.01, ctMsg, 'not known for HUAC', true, true);
				}
			}

			try
			{
				if (TLSState.protocolVersion < 1)
				{
					TLSObjectAdd(0.01, 0.01, 'TLS Version', TLSState.protocolVersion + " (SSL)", 1);
				}
				else
				if (TLSState.protocolVersion < 3)
				{
					if (TLSState.protocolVersion == 2)
						TLSObjectAdd(0.8, 0.4, 'TLS Version', TLSState.protocolVersion + " (TLS 1.1)", 1);
					else
					//if (TLSState.protocolVersion == 1)
						TLSObjectAdd(0.3, 0.1, 'TLS Version', TLSState.protocolVersion + " (TLS 1.0)", 1);
				}
				else
				{
					if (TLSState.protocolVersion == 3)
						TLSObjectAdd(false, false, 'TLS Version', TLSState.protocolVersion + " (TLS 1.2)");
					else
					if (TLSState.protocolVersion == 4)
						TLSObjectAdd(false, false, 'TLS Version', TLSState.protocolVersion + " (TLS 1.3)");
					else
						TLSObjectAdd(false, false, 'TLS Version', TLSState.protocolVersion + " (TLS ???)");
				}
			}
			catch (e)
			{
				TLSObjectAdd(0.01, 0.01, 'TLS Version', 'error', true, true);
			}

			try
			{
				TLSObject.protocolVersion = TLSState.protocolVersion;
				TLSObject.secretKeyLength = TLSState.secretKeyLength;
				TLSObject.cipherName      = TLSState.cipherName;
			}
			catch (e)
			{
				TLSObjectAdd(0.001, 0.001, 'error', e.message, true, true);
				return TLSObject;
			}


			if (TLSState.secretKeyLength >= 256)
				TLSObjectAdd(false, false, 'Symmetric cipher key length', TLSState.secretKeyLength);
			else
				TLSObjectAdd(TLSState.secretKeyLength, 0.0, 'Symmetric cipher key length', TLSState.secretKeyLength);


			var uCBC = TLSState.cipherName.indexOf('_CBC_');
			var uGCM = TLSState.cipherName.indexOf('_GCM_');
			var uRC4 = TLSState.cipherName.indexOf('_RC4_');
			// security.ssl3.ecdhe_ecdsa_chacha20_poly1305_sha256 security.ssl3.ecdhe_rsa_chacha20_poly1305_sha256
			var uChaCha20 = TLSState.cipherName.indexOf('_CHACHA20_');

			if (uCBC >= 0 || uRC4 >= 0)
			{
				TLSObject.blockCipherOrRC4 = 0;
			}
			else
			if (uGCM >= 0)
				TLSObject.blockCipherOrRC4 = 1;
			else
			if (uChaCha20 >= 0)
				TLSObject.blockCipherOrRC4 = 3;
			else
				TLSObject.blockCipherOrRC4 = 2;

			if (TLSObject.blockCipherOrRC4 == 0)
			{
				if (uRC4 >= 0)
				{
					TLSObjectAdd(32, 0.001, 'Symmetric cipher', 'RC4');
				}
				else
				{
					TLSObjectAdd(0.95, 0.85, 'Symmetric cipher', 'CBC', 1);
				}
			}
			else
			if (TLSObject.blockCipherOrRC4 == 1)
			{
				TLSObjectAdd(false, false, 'Symmetric cipher', 'GCM');
			}
			else
			if (TLSObject.blockCipherOrRC4 == 3)
			{
				TLSObjectAdd(false, false, 'Symmetric cipher', 'ChaCha20');
			}
			else
				TLSObjectAdd(0.01, 0.01, 'Symmetric cipher', 'unknown');

			var PFS = false;
			var uDHE   = TLSState.cipherName.indexOf('_DHE_');
			var uECDHE = TLSState.cipherName.indexOf('_ECDHE_');

			if (uDHE >= 0 || uECDHE >= 0)
			{
				if (uECDHE >= 0)
					TLSObjectAdd(false, false, 'Key exchange', 'ECDHE (? bit)');
				else
				{
					TLSObjectAdd(this.getEqRSA(2048), this.getEqRSA(2048), 'Key exchange', 'DHE (? bit)', false, false);
				}

				PFS = true;
			}
			else
			{
				TLSObjectAdd(0.8, 0.25, 'Key exchange', 'No PFS |ECDHE or DHE|', 1);
			}

			var uSHA1   = TLSState.cipherName.indexOf('_SHA');
			if (uSHA1 > 0 && TLSState.cipherName.length - 3 <= uSHA1)
				uSHA1 = -1;

			var uSHA256  = TLSState.cipherName.indexOf('_SHA256');
			var uSHA384  = TLSState.cipherName.indexOf('_SHA384');
			var uSHA512  = TLSState.cipherName.indexOf('_SHA512');
			var poly1305 = TLSState.cipherName.indexOf('_POLY1305');
			var uHashL   = this.getHashL(TLSState.cipherName);


			// +8.5 - это плюс к стойкости хеша, так как он краткосрочный
			// +4 + 13 - +4 это просто добавление к рассчёту ниже на разницу между 1024 и 64. 13 - добавление на канал связи.
			// С другой стороны, сравнивать принципиально ограниченный канал связи с неограниченными вычислительными возможностями,
			// возможно, не стоит. Или стоит добавить ещё так битов 16-ть на то, что возможности спецслужб по обращению к конкретному
			// серверу значительно ниже, чем по вычислительным мощностям.
			var cadd = 8.5;
			var hashStrong = function(uHashL)
			{
				// uHashL[0] - номинальная стойкость
				// uHashL[1] - с понижающим коэффициентом
				// Берём так, чтобы всё просто было
				var a = uHashL[1] + cadd;//uHashL[0]/2 + 4 + 13 + cadd;
				var b = uHashL[1] + cadd;

				return Math.min(a, b);
			};


			if (poly1305 >= 0)
			{
				// 97 битов - это когда дробь вероятности "The Poly1305-AES message-authentication code" даёт число 1,0
				// для сообщения L=1024, количества сообщений C=1, количества ложных сообщений D=97
				// 4 бита - разница между хешем 1024 байтом и хешем 64 байтов (ведь sha-2 хеширует по 64 байта)
				// Хотя, с другой стороны, эту разницу, возможно, лучше не учитывать
				// Т.к. стойкость хеша sha2, по идее, не должна особо сильно снижаться при увеличении длины сообщения
				// Хотя poly1305 не очень стойкий, он использует одноразовый ключ, подбор которого, фактически,
				// затруднён стойкостью алгоритма выбора ключа
				// Поэтому его подбор должен опираться на перебор большого количества вариантов ложных сообщений
				// в интерактивном режиме, то есть отсылкой ложных сообщений на компьютер клиента.
				// В результате можем добавить повышенную стойкость к перебору. Считая, что среднее сообщение у нас
				// длиной в 1024 байта, на пропускном канале 1ГБит мы можем подобрать 2^30/8/1024=2^(30-3-10)=2^17 сообщений в секунду.
				// Это 131072 сообщений в секунду. В то время, как, скажем, 128-битный MD5 на компьютере противника перебирается
				// миллиардами вариантов (допустим, возьмём 16 миллиардов - 2^34 делим на 1024/64=16, получаем 2^30)
				// Разница как 2^17 и 2^30. То есть имеем ещё 30-17=13 битов ключа в запас
				//TLSObjectAdd(97 + 4 + 13 + cadd, 0.0, 'Hash for symmetric cryptography', 'poly1305', false, true);
				// В общем, считаем, что нужно эту хрень перебирать только полным перебором

				var sPoly = 128 + cadd;
				TLSObjectAdd(Math.min(sPoly, hashStrong(uHashL)), 0.0, 'MAC for symmetric cryptography', 'poly1305 + PRF ' + uHashL[2], false, true);
			}
			else
			if (uGCM >= 0)
			{
				// GCM в AES применяется с t=96 битов (правда, я нашёл только IPsec)
				// Примерная стойкость, 2^63 операции отсылки пакетов
				// https://tools.ietf.org/html/rfc5288#ref-GCM
				// Note that each of these AEAD algorithms uses a 128-bit authentication tag with GCM
				var sGCM = 128 + cadd;
				TLSObjectAdd(Math.min(sGCM, hashStrong(uHashL)), 0.0, 'MAC for symmetric cryptography', 'GCM + PRF ' + uHashL[2], false, true);
			}
			else
			// Счиатаем, нахождение прообраза требует 2^n, нахождение коллизий требует 2^(n/2+13)
			if (uSHA256 >= 0 || uSHA384 >= 0 || uSHA512 >= 0)
			{
				let macscMsg = 'MAC for symmetric cryptography';
				if (uSHA256 >= 0)
					TLSObjectAdd(hashStrong(uHashL), 0.0, macscMsg, uHashL[2], false, true);
				else
				if (uSHA384 >= 0)
					TLSObjectAdd(hashStrong(uHashL), 0.0, macscMsg, uHashL[2], false, true);
				else
				if (uSHA512 >= 0)
					TLSObjectAdd(hashStrong(uHashL), 0.0, macscMsg, uHashL[2], false, true);
				else
				{
					TLSObjectAdd(112*cSHA1, 0.0, macscMsg, 'SHA-2 UNKNOWN');
					console.error('HUAC: symmetric cipher with SHA UNKNOWN');
				}
			}
			else
			// https://www.schneier.com/blog/archives/2012/10/when_will_we_se.html
			// НО. Считаем, что взлом 57-мибитного хеша обойдётся на 1 января 2017 года в 50 тысяч баксов
			// 57*2=104; 80+13=93
			if (uSHA1 >= 0)
			{
				uHashL = this.getHashL(' SHA-1');
				TLSObjectAdd(hashStrong(uHashL), 0.7, 'MAC for symmetric cryptography', 'SHA-1', false, true);
			}
			else
			{
				TLSObjectAdd(0.1, 0.1, 'MAC for symmetric cryptography', 'unknown', true, true);
			}

/*

https://dxr.mozilla.org/mozilla-central/source/security/manager/ssl/nsISSLStatus.idl

Since FF 52
https://bugzilla.mozilla.org/show_bug.cgi?id=1304924
sec->authType = ssl_auth_null;
sec->authKeyBits = 0;
sec->signatureScheme = ssl_sig_none;
sec->keaType = ssl_kea_null;
sec->keaKeyBits = 0;
sec->keaGroup = NULL;

	 inf.keaType = ss->sec.keaType;
     inf.keaGroup = ss->sec.keaGroup ? ss->sec.keaGroup->name : ssl_grp_none;
	 inf.keaKeyBits = ss->sec.keaKeyBits;
	 inf.authType = ss->sec.authType;
	 inf.authKeyBits = ss->sec.authKeyBits;
     inf.signatureScheme = ss->sec.signatureScheme;
	inf.signatureScheme = ss->sec.signatureScheme;

typedef enum {
    ssl_sig_none = 0,
    ssl_sig_rsa_pkcs1_sha1 = 0x0201,
    ssl_sig_rsa_pkcs1_sha256 = 0x0401,
    ssl_sig_rsa_pkcs1_sha384 = 0x0501,
    ssl_sig_rsa_pkcs1_sha512 = 0x0601,
    // For ECDSA, the pairing of the hash with a specific curve is only enforced
    // in TLS 1.3; in TLS 1.2 any curve can be used with each of these.
    ssl_sig_ecdsa_secp256r1_sha256 = 0x0403,
    ssl_sig_ecdsa_secp384r1_sha384 = 0x0503,
    ssl_sig_ecdsa_secp521r1_sha512 = 0x0603,
    ssl_sig_rsa_pss_sha256 = 0x0804,
    ssl_sig_rsa_pss_sha384 = 0x0805,
    ssl_sig_rsa_pss_sha512 = 0x0806,
    ssl_sig_ed25519 = 0x0807,
    ssl_sig_ed448 = 0x0808,

    ssl_sig_dsa_sha1 = 0x0202,
    ssl_sig_dsa_sha256 = 0x0402,
    ssl_sig_dsa_sha384 = 0x0502,
    ssl_sig_dsa_sha512 = 0x0602,
    ssl_sig_ecdsa_sha1 = 0x0203,

   // The following value (which can't be used in the protocol), represents
   // the RSA signature using SHA-1 and MD5 that is used in TLS 1.0 and 1.1.
   // This is reported as a signature scheme when TLS 1.0 or 1.1 is used.
   // This should not be passed to SSL_SignatureSchemePrefSet(); this
   // signature scheme is always used and cannot be disabled. 
    ssl_sig_rsa_pkcs1_sha1md5 = 0x10101,
} SSLSignatureScheme;
*/

			var cert = TLSState.serverCert;
			if (!cert)
			{
				//console.error('no cert');
				TLSObjectAdd(0.0, 0.0, 'ERROR', 'no certificate found', true, true);
				return TLSObject;
			}

			cert.validity.QueryInterface(Ci.nsIX509CertValidity);
/*
			var toStr = function(obj, toObj, name, level)
			{
				if (level > 3)
					return;

				for (var c in obj)
				{
					try
					{
						if (obj[c].substring)
						{
							if (!toObj[name])
								toObj[name] = {};

							toObj[name][c] = obj[c];
						}
						else
						if (obj[c] instanceof Object)
						{
							var t = {};
							toStr(obj[c], t, c, level + 1);
							if (t[c])
								toObj[name][c] = t[c];
						}
					}
					catch (e)
					{}
				}
			}
			*/
			//TLSObject.cert = {};
			//toStr(cert, TLSObject, 'cert', 0);

			var rCerts = [];
			var sCerts = [];
			var crt = cert;
			var noFirst = false;
			var cid = cert;

			var certsHierarchyCount = 0;
			var chcCur = cert;
			while (chcCur && certsHierarchyCount < 256)
			{
				chcCur = chcCur.issuer;
				certsHierarchyCount++;
			}

			do
			{
				if (crt instanceof Ci.nsIX509Cert)
				{
					var certificates = Cc["@mozilla.org/security/nsASN1Tree;1"].createInstance(Ci.nsIASN1Tree);
					certificates.loadASN1Structure(crt.ASN1Structure);

					
					var cf = this.getCertF(certificates, crt, TLSObjectAddA, PFS || noFirst, !noFirst, rCerts.length, certsHierarchyCount);
					rCerts.push(cf);

					var sCert = {
						sha2: crt.sha256Fingerprint,
						sha256SubjectPublicKeyInfoDigest: crt.sha256SubjectPublicKeyInfoDigest,
						num: cert.serialNumber,
						name: (crt.commonName || crt.windowTitle || crt.organization),
						notAfter: crt.validity.notAfter,
						notBefore: crt.validity.notBefore,
						fh: cf.h.f,
						fs: cf.s.f
					};

					if (hpkpdigest)
						TLSObjectAdd(false, false, sCert.sha256SubjectPublicKeyInfoDigest + ': ' + sCert.name, 'debug info');

					if (isHPKP['Public-Key-Pins'] && isHPKP['Public-Key-Pins'].indexOf(crt.sha256SubjectPublicKeyInfoDigest) >= 0)
					{
						sCert.hpkp = true;
					}

					sCerts.push(sCert);

					noFirst = true;
				}
				else
				{
					console.error("Http UserAgent Cleaner: NO nsIX509Cert");

					TLSObjectAdd(0.0, 0.0, 'ERROR', 'no X509Cert certificate found', true, true);
					return TLSObject;
				}

				crt = crt.issuer;

				if (crt)
					cid = crt;
			}
			while (crt);

			if (!cert.issuer && !cert.isSelfSigned)
			{
				console.error('HUAC ERROR: TLSlog: !cert.issuer && !cert.isSelfSigned');
				console.error(httpChannel.URI.spec);
				console.error(cert);
				TLSObjectAdd(0.0, 0.0, 'Incorrect internal information', 'ERROR', true, true, true);
			}

			TLSObject.fHPKP = 1.0;
			if (isHPKP.HPKP || isHPKP.haveContent)
			{
				if (isHPKP.HPKP)
				{
					var a = sCerts.length - 1;
					for (var i = sCerts.length - 1; i >= 0; i--)
					{
						if (sCerts[i].hpkp)
						{
							a = i;
							break;
						}
					}

					// Экспоненциальное распределение
					var pHPKP = 1.0 - Math.exp(-isHPKP.HPKP/12);
					var sHPKP = 1.0;
					if (a == 1)
						sHPKP = 0.45;
					else
					if (a > 1)
						sHPKP = 0.3;

					// Берём, что на сайт человек может не заходить из-за отпуска 30 дней
					if (isHPKP.HPKP <= 34)
						pHPKP *= 0.5;
					else
					{
						// Считаем, что за два месяца он уж точно зайдёт
						if (isHPKP.HPKP < 60)
						{
							var pCorrection = 1.0 - Math.exp((30-isHPKP.HPKP)/6);
							if (pCorrection > 0.5)
								pHPKP *= pCorrection;
							else
								pHPKP *= 0.5;
						}
					}

					if (pHPKP < 0)
						pHPKP = 0.0;
					else
					if (pHPKP > 1.0)
						pHPKP = 1.0;

					var aStr = '?';
					if (a == 0)
						aStr = 'Server';
					else
					if (a == sCerts.length - 1)
						aStr = 'Root';
					else
					if (a > 0)
						aStr = 'Intermediate ' + a;


					TLSObjectAdd(pHPKP, pHPKP, 'HPKP lifetime', Math.floor(isHPKP.HPKP), true, true, true);
					TLSObjectAdd(sHPKP, sHPKP, 'HPKP certificate', aStr, true, true, true);
				}
				else
					TLSObjectAdd(0.0, 0.0, 'HPKP', false, true, true, true);
			}
			else
				TLSObject.fHPKP = false;

			
			var fm  = [];
			var fml = [];
			TLSObject.certMsg = [];
			//for (var certResult of rCerts)
			var certResultLast = null;
			for (var i = rCerts.length - 1; i >= 0; i--)
			{
				// certResult.h - хеш
				// certResult.s - подпись
				var certResult = rCerts[i];
				var fmA  = certResult.h.f;
				var fmlA = certResult.h.flong;

				if (certResultLast)
				{
					fmA  *= certResultLast.s.f;
					fmlA *= certResultLast.s.flong;
				}

				fm .push(fmA);
				fml.push(fmlA);

				certResultLast = certResult;

				// console.error(certResult);
				//TLSObject.certMsg.push(certResult.s);
				//TLSObject.certMsg.push(certResult.h);
			}
			for (var i = 0; i < rCerts.length; i++)
			{
				var certResult = rCerts[i];
				TLSObject.certMsg.push(certResult.st);
				TLSObject.certMsg.push(certResult.s);
				TLSObject.certMsg.push(certResult.h);
			}

			var fma  = 1.0;
			var fmla = 1.0;
			for (var f of fm)
				if (fma > f)
					fma = f;
			for (var f of fml)
				if (fmla > f)
					fmla = f;

			// Учитываем вероятность хищения сертификатов как произведение вероятностей
			for (var i = rCerts.length - 1; i >= 0; i--)
			{
				var certResult = rCerts[i];
				fma  *= certResult.st.f;
				fmla *= certResult.st.flong;
			}

			TLSObjectAdd(fma, fmla, 'Signature and signature hash', '', 1);

			// console.error(TLSObject);

			if (certsHostsTracking)
			{
				TLSObject.huacId = cid.serialNumber + '/' + cid.sha1Fingerprint + '/' + cid.sha256Fingerprint;
			}
			else
				TLSObject.huacId = false;
			
			TLSObject.sCerts = sCerts;

			return TLSObject;
	},
	
	getSignatureAlgNameFromCertASN1: function(cert)
	{
		var certAlg = '';
		var key     = 0;
		try
		{
			switch (cert.getDisplayData(11))
			{
				case pipnss.GetStringFromName('CertDumpRSAEncr'):
					certAlg = 'RSA';
					key = cert.getDisplayData(12).replace(/[^0-9() ]/g, '').replace(/[^0-9]+/g, '|').split('|')[1];
					key = Number(key);
					break;
			}
		}
		catch (e)
		{
			console.error(e);
		}

		if (!certAlg)
		{
			try
			{
				switch (cert.getDisplayData(12))
				{
					case pipnss.GetStringFromName('CertDumpECPublicKey'):
						certAlg = 'ECC';
						key = cert.getDisplayData(14).replace(/[^0-9() ]/g, '').replace(/[^0-9]+/g, '|').split('|')[1];
						key = Number(key);
						break;
					case pipnss.GetStringFromName('CertDumpAnsiX9DsaSignature'):
					case pipnss.GetStringFromName('CertDumpAnsiX9DsaSignatureWithSha1'):
						certAlg = 'DSA';
						// key = cert.getDisplayData(14).replace(/[^0-9() ]/g, '').replace(/[^0-9]+/g, '|').split('|')[1];
						break;
				}
			}
			catch (e)
			{
				console.error(e);
			}
		}

		return [certAlg, key];
	},
	
	CleanLog: function()
	{
		var tabs = this.urls.tabs;
		var I    = this.urls.utils;

		for (var ti in this.tabs)
		{
			
			var t = this.tabs[ti];
			var found = false;
			for (var tabIndex in tabs)
			{
				try
				{
					var tab = tabs[tabIndex];
					var taburl = this.urls.fromTabDocument(this.urls.sdkTabToChromeTab(tab)).split('#')[0];
					if (t.tabUri == this.urls.getURIWithoutScheme(taburl))
					{
						found = true;
						break;
					}
				}
				catch (e)
				{
					console.error(e);
				}
			}

			if (found)
				continue;

			// console.error("deleted " + t.tabUri);
			this.tabs.splice(ti, 1);
		}

		// console.error(this.tabs);
	},

	CleanAll: function(onlyPrivate)
	{
		if (onlyPrivate)
			this.CleanLog();
		else
			this.tabs = [];
	},

	FindTab: function(tabUrl)
	{
		if (!tabUrl)
			return null;

		tabUrl = this.urls.getURIWithoutScheme(tabUrl.split('#')[0]);

		for (var ti in this.tabs)
		{
			var t = this.tabs[ti];
			if (t.tabUri == tabUrl)
			{
				return t;
			}
		}
		
		return null;
	},

	addToLog: function(tab, tabUri, obj, tabHost, url, TLSInfo)
	{
		var redirectTo = obj.redirectTo;

		if (!tab || !tabHost)
			return;

		var tabURISpec = tab.huac ? tab.huac.url : tabUri;
		/*if (tab.linkedBrowser && tab.linkedBrowser.HTTPUACleaner_URI)
		{
			tabURISpec = tab.linkedBrowser.HTTPUACleaner_URI.spec ? tab.linkedBrowser.HTTPUACleaner_URI.spec : tab.linkedBrowser.HTTPUACleaner_URI;
			console.error('tab.linkedBrowser.HTTPUACleaner_URI.spec ' + tabURISpec);
		}*/

		if (!tabURISpec || tabURISpec == 'about:blank' || tabURISpec == 'about:newtab' || tabURISpec.indexOf('about:neterror') >= 0)
		{
			return;
		}
	/*
	    if (tabURISpec == 'about:blank' || tabURISpec == 'about:newtab')
			tabURISpec = url;
		*/
		tabURISpec = this.urls.getURIWithoutScheme(tabURISpec.split('#')[0]);

		if (redirectTo)
		{
			redirectTo = this.urls.getURIWithoutScheme(redirectTo.split('#')[0]);
			// console.error('redirectTo: ' + redirectTo);
		}

		var tabFound = null;
		for (var ti in this.tabs)
		{
			var t = this.tabs[ti];
			if (!t.redirected && t.tabUri == tabURISpec)
			{
				if (/*(Date.now() - t.startTime)/(t.endTime - t.startTime + 15000) > 2.0*/ tabURISpec == this.urls.getURIWithoutScheme(url.split('#')[0]) )
				{
					//console.error('deleted (!rt) ' + url); console.error(tabURISpec);console.error(t);
					this.tabs.splice(ti, 1);
					continue;
				}
			}
			
			if (redirectTo && t.tabUri == redirectTo)
			{
				//console.error('deleted (rt) ' + url); console.error(tabURISpec);console.error(t);
				this.tabs.splice(ti, 1);
				continue;
			}
		}
		
		for (var ti in this.tabs)
		{
			var t = this.tabs[ti];
			if (t.tabUri == tabURISpec)
			{
				tabFound = t;
				break;
			}
		}

		if (!TLSInfo)
			TLSInfo = {f: 0.0, flong: 0.0};

		TLSInfo.url = url;
		if (!tabFound || obj.topdocument)
		{
			var url0 = this.urls.getURIWithoutScheme(url.split('#')[0]);
			if (tabURISpec == url0 || obj.topdocument)
			{
				// Замена производится на url редиректа, однако бывает так, что замена производится на esia.gosuslugi.ru:443/idp/AuthnEngine
				// а реальный url вкладки получается esia.gosuslugi.ru/idp/AuthnEngine
				if (redirectTo)
				{
					tabURISpec = redirectTo;
					//console.error('tabURISpec = redirectTo: ' + redirectTo);
				}
				else
				if (obj.topdocument)
				{
					tabURISpec = url0;
				}

				tabFound = {tabUri: tabURISpec, redirected: !!redirectTo, tabHost: tabHost, TLSInfo: [TLSInfo], startTime: Date.now(), endTime: Date.now()};
				this.tabs.push(tabFound);

				// console.error('added ' + url); console.error(tabURISpec); console.error(this.tabs);
			}
			else
			{
				/*console.error('skipped ' + url);
				console.error('tabURISpec ' + tabURISpec);*/
				/*
				if (tabURISpec == this.urls.getURIWithoutScheme(url.split('#')[0]))
					console.error('!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!');*/
			}
		}

		// На всякий случай логируем obj.topdocument и сюда, и в новую вкладку
		if (tabFound)
		{
			if (redirectTo)
			{
				tabFound.tabUri = redirectTo;
				tabFound.redirected = true;
				//console.error('tabFound.tabUri = redirectTo: ' + redirectTo);
			}
			else
				tabFound.redirected = false;
			
			if (tabFound.TLSInfo.length > 64)
			{
				if (!tabFound.truncated)
				{
					tabFound.truncated = true;
					tabFound.tcount    = 1;
				}

				if (tabFound.truncated)
				{
					var pushed = false;
					if (tabFound.f > TLSInfo.f)
					{
						tabFound.f = TLSInfo.f;
						tabFound.TLSInfo.push(TLSInfo);
						pushed = true;
					}
					if (tabFound.flong > TLSInfo.flong)
					{
						tabFound.flong = TLSInfo.flong;
						if (!pushed)
						{
							tabFound.TLSInfo.push(TLSInfo);
							pushed = true;
						}
					}
					
					if (!pushed)
						tabFound.tcount++;
				}
			}
			else
				tabFound.TLSInfo.push(TLSInfo);

			tabFound.endTime = Date.now();

			//console.error('found ' + url); console.error(tabURISpec);
		}
		// console.error(this.tabs);
	},
	
	getCertificateObject_Cache: {},
	getCertificateObject: function(X509Cert)
	{/*
		if (X509Cert.sha256Fingerprint)
		if (this.getCertificateObject_Cache[X509Cert.sha256Fingerprint])
			return Object.create(this.getCertificateObject_Cache[X509Cert.sha256Fingerprint]);
*/
		var certResult = {};
		try
		{
			var cert = null;
			if (X509Cert.QueryInterface)
				cert = X509Cert.QueryInterface(Ci.nsIX509Cert);
			else
				cert = X509Cert;

			certResult.usages   = {nousage: true, root: false, server: false, mail: false, object: false};
			certResult.usages.server = this.CertDb.isCertTrusted(cert, cert.certType, this.CertDb.TRUSTED_SSL);
			certResult.usages.mail   = this.CertDb.isCertTrusted(cert, cert.certType, this.CertDb.TRUSTED_EMAIL);
			certResult.usages.object = this.CertDb.isCertTrusted(cert, cert.certType, this.CertDb.TRUSTED_OBJSIGN);

			if (certResult.usages.server || certResult.usages.mail || certResult.usages.object)
			{
				certResult.usages.nousage = false;
			}
			else
				certResult.usages.nousage = true;

			if (this.getCertificateObject_Cache[cert.sha256Fingerprint])
			{
				this.getCertificateObject_Cache[cert.sha256Fingerprint].usages = certResult.usages;
				return Object.create(this.getCertificateObject_Cache[cert.sha256Fingerprint]);
			}

			var certificates = Cc["@mozilla.org/security/nsASN1Tree;1"].createInstance(Ci.nsIASN1Tree);
			certificates.loadASN1Structure(cert.ASN1Structure);

			var signatureAlg = this.getSignatureAlgNameFromCertASN1(certificates);
			var encryptionString = certificates.getDisplayData(4).replace(/PKCS #1/g, '').replace(/Encryption/g, '');
			var hashLA = this.getHashL(encryptionString, signatureAlg[0]);

			certResult.wt   = cert.windowTitle;
			certResult.cn   = cert.commonName;
			certResult.sn   = cert.serialNumber;
			certResult.sha1 = cert.sha1Fingerprint;
			certResult.sha2 = cert.sha256Fingerprint;
			certResult.algn = signatureAlg[0];
			certResult.algl = signatureAlg[1];
			certResult.algh = hashLA[2];
			certResult.cert = cert;
			certResult.certType = cert.certType;

			certResult.organization  = cert.organization;
			certResult.organizationU = cert.organizationalUnit;
			certResult.subjectName   = cert.subjectName;
			certResult.huacId        = cert.serialNumber + '/' + certResult.sha1 + '/' + certResult.sha2;

			var getCertName = function(cert)
			{
				var r = cert.commonName || cert.windowTitle;
				
				if (!r)
					r = cert.organization;
				
				if (r.indexOf(':') > 0)
				{
					var a = r.indexOf(':');
					r = r.substring(a + 1);
				}
				
				return r;
			};
			
			certResult.name = getCertName(cert);
			if (cert.issuer)
			{
				certResult.issuerName = getCertName(cert.issuer);
			}
			else
				certResult.issuerName = cert.issuerOrganization || cert.issuerCommonName;


			// https://dxr.mozilla.org/firefox/source/security/manager/ssl/public/nsIX509CertDB.idl
			// setCertTrust
			// isCertTrusted
			// verifyCertNow
			// http://dxr.mozilla.org/firefox/source/security/nss/lib/certdb/certt.h
			// SECCertificateUsage	certificateUsageSSLServer	0x0002
			// certificateUsageSSLCA	0x0008
			// certificateUsageVerifyCA	0x0100
			// certificateUsageAnyCA	0x0800
			// certificateUsageCheckAllUsages 0
			// http://dxr.mozilla.org/mozilla-central/source/security/manager/ssl/public/nsIX509CertDB.idl#346
		}
		catch (e)
		{
			console.error(e);
			return null;
		}

		this.getCertificateObject_Cache[cert.sha256Fingerprint] = certResult;
		return Object.create(certResult);
	},

	cachedTime: Date.now(),
	resetCachedDataByTime: function(reset)
	{
		if (reset || Date.now() - this.cachedTime > 86400000) // 24*60*60*1000
		{
			this.getCertificates_Cache = [];
			this.getCertificateByHuacId_Cache = {};
			this.cachedTime = Date.now();
		}
	},

	getCertificates_Cache: [],
	getCertificates: function()
	{
		this.resetCachedDataByTime();

		var result = [];
		if (this.getCertificates_Cache.length > 0)
		{
			for (var crt of this.getCertificates_Cache)
				result.push(this.getCertificateObject(crt));

			return result;
		}

		var certs = this.CertDb.getCerts().getEnumerator();
		while (certs.hasMoreElements())
		{
			var cert = certs.getNext();
			var crt = this.getCertificateObject(cert);
			result.push(Object.create(crt));

			this.getCertificates_Cache.push(cert);
		}

		return result;
	},

	getCertificateByHuacId_Cache: {},
	getCertificateByHuacId: function(huacId)
	{
		this.resetCachedDataByTime();

		var cache = false;
		if (this.getCertificates_Cache.length <= 0)
			cache = true;

		if (this.getCertificateByHuacId_Cache[huacId] || this.getCertificateByHuacId_Cache[huacId] === false)
		{
			return this.getCertificateByHuacId_Cache[huacId];
		}

		var result = false;
		var certs = this.CertDb.getCerts().getEnumerator();
		while (certs.hasMoreElements())
		{
			//result.push(  this.getCertificateObject( certs.getNext() )  );

			var cert = certs.getNext();
			cert = cert.QueryInterface(Ci.nsIX509Cert);
			var huacIdA = cert.serialNumber + '/' + cert.sha1Fingerprint + '/' + cert.sha256Fingerprint;

			if (!this.getCertificateByHuacId_Cache[huacIdA])
				this.getCertificateByHuacId_Cache[huacIdA] = cert;

			if (cache)
				this.getCertificates_Cache.push(cert);

			if (huacIdA == huacId)
			{
				// this.getCertificateByHuacId_Cache[huacId] = cert;
				result = cert;
			}
		}

		this.getCertificateByHuacId_Cache[huacId] = result;
		return result;
	}
};
