//const {Cc, Ci, Cu, Cr, Cm, components} = require("chrome");
//var pipnss  = Cc['@mozilla.org/intl/stringbundle;1'].getService(Ci.nsIStringBundleService).createBundle('chrome://pipnss/locale/pipnss.properties');

HTTPUACleaner.certs = function(fileName, callback)
{
	this.fileMutex      = new HTTPUACleaner.mutex(2);
	this.fileMutex.name = 'fileMutex.certs';

	this.sortType  = 0;
	this.sortTypeHosts = 0;
	this.viewType  = 2;
	this.sortTypeCount = 4;
	this.viewTypeCount = 4;
	this.initialized   = false;
	
	this.lastChanged = {serious: 0, all: 0};
	this.hostsOpt    = {autodisable: false};
	
	this.keepAliveOptionChanged();

	this.rootCertificateFirstDisableCounter = false;

	// OS загружено в файле \data\mainjs\settingsDb.js
	this.dir      = OS.Path.join(OS.Constants.Path.profileDir, 'HTTPUACleaner');
	this.fileName = OS.Path.join(this.dir, fileName);
	this.decoder  = new TextDecoder();
	this.encoder  = new TextEncoder();  

	var promise = OS.File.makeDir(this.dir, {ignoreExisting: true});

	let t = this;
	
	promise.then
	(
		function()
		{
			t.loadSettings
			(
				function(result, str)
				{
					t.initialized = true;
					if (callback)
						callback(result);

					t.setInterval();
				}
			);
		},

		function()
		{
			if (callback)
				callback(false);
		}
	).catch(console.error);
};

HTTPUACleaner.certs.prototype.setInterval = function()
{
	this.unknownHosts = {};
	if (!this.hostsOpt.autodisable)
	{
		this.clearInterval();
		return;
	}

	if (this.intervalId)
		return;


	var t = this;
	this.intervalId = HTTPUACleaner.timers.setInterval
	(
		function()
		{
			t.rootCertificateDisable();
		},
		983
	);
};

HTTPUACleaner.certs.prototype.clearInterval = function(uninstall)
{
	this.unknownHosts = {};
	if (!this.intervalId && !uninstall)
		return;

	if (this.intervalId)
	{
		HTTPUACleaner.timers.clearInterval(this.intervalId);
		this.intervalId = false;
	}

	this.rootCertificateFirstDisableCounter = false;
};

HTTPUACleaner.certs.prototype.keepAliveOptionChanged = function()
{/*
	'network.tcp.keepalive.enabled'
	'network.http.tcp_keepalive.short_lived_connections'
	'network.http.tcp_keepalive.long_lived_connections'
	'network.tcp.keepalive.idle_time'
	'network.http.keep-alive.timeout'
	'network.http.tcp_keepalive.long_lived_idle_time'
	'network.http.tcp_keepalive.short_lived_idle_time'
	'network.http.tcp_keepalive.short_lived_time'
	*/
	var prefs = HTTPUACleaner['sdk/preferences/service'];

	var times = [];
	if (prefs.get('network.http.tcp_keepalive.short_lived_connections'))
	{
		times.push(prefs.get('network.http.tcp_keepalive.short_lived_idle_time'));
		times.push(prefs.get('network.http.tcp_keepalive.short_lived_time'));
	}

	if (prefs.get('network.http.tcp_keepalive.long_lived_connections'))
	{
		times.push(prefs.get('network.http.tcp_keepalive.long_lived_idle_time'));
	}

	times.push(prefs.get('network.tcp.keepalive.idle_time'));
	times.push(prefs.get('network.http.keep-alive.timeout'));

	var max = 0;
	for (var time of times)
	{
		if (time > max)
			max = time;
	}

	this.maxKeepAliveTime = max * 1000;
};

HTTPUACleaner.certs.prototype.saveSettingDecision = function(now, seriousChanges, fromInterval)
{
	if (!this.lastChanged)
		this.lastChanged = {all: false, serious: false, last: 0};

	if (!fromInterval)
	{
		if (!this.lastChanged.all)
			this.lastChanged.all = now;

		this.lastChanged.last = now;

		if (seriousChanges)
		{
			if (!this.lastChanged.serious)
				this.lastChanged.serious = now;
		}
	}

	if (
		this.lastChanged.all && now - this.lastChanged.all > 1 * 60 * 1000
		|| (seriousChanges && this.lastChanged.serious && now - this.lastChanged.serious > 1 * 1000)
		|| now - this.lastChanged.last > 15 * 1000
		)
	{
		this.lastChanged.last    = 0;
		this.lastChanged.all     = false;
		this.lastChanged.serious = false;

		this.saveSettings();
	}
};

HTTPUACleaner.certs.prototype.saveSettings = function(callback)
{
	if (HTTPUACleaner.terminated)
	{
		console.error('HUAC warning: save (certs settings) cancelled because HUAC terminated');
		return;
	}

	var t = this;

	let settings = this.options;

	var cb = function()
	{
		this.saveSettings(callback);
	};

	if (!this.fileMutex.enter(cb, this))
		return;

	try
	{
		this.save
		(
			settings,
			function(result)
			{
				try
				{
					if (callback)
						callback(result, t);
				}
				catch(e)
				{
					HTTPUACleaner.logObject(e, true);
				}

				t.fileMutex.release();
			}
		);
	}
	catch (e)
	{
		HTTPUACleaner.logMessage('HUAC FATAL ERROR: Save settings failed: call load error', true);
		HTTPUACleaner.logObject(e, true);

		this.fileMutex.release();
	}

	return true;
};

HTTPUACleaner.certs.prototype.save = function(toSave, callback)
{
	var promise = OS.File.open(this.fileName, {existing: false, read: false, write: true, append: false, truncate: true});
	
	var t = this;
	
	// JSON.stringify
	// JSON.parse
	var write = function(file)
	{
		var toSaveStr = JSON.stringify(toSave);
		let now = Date.now();

		// --------------------
		try
		{
			if (!toSave.lastSave || now - toSave.lastSave.time > 1000*3600*24*30 || toSaveStr.length/toSave.lastSave.size > 1.189)
			{
				toSave.lastSave = {time: now, size: toSaveStr.length + 1};// Чтобы выше не было деления на ноль
				toSaveStr = JSON.stringify(toSave);
				let encoded = t.encoder.encode(toSaveStr);

				var copyFileName = t.fileName + '.' + now + '.copy';
				let copyPromise = OS.File.writeAtomic(copyFileName, encoded, {tmpPath: copyFileName + '.tmp', noOverwrite: false, flush: true});
				copyPromise.then
				(
					function onSuccess()
					{},
					function onFailure(reason)
					{
						console.error(reason);

						HTTPUACleaner.logMessage('save error (HTTPUACleaner.sdbP.prototype.save g009bkS6KJlm)');
						HTTPUACleaner.logObject(reason);
					}
				).catch
				(
					function(e)
					{
						HTTPUACleaner.logMessage('save error (HTTPUACleaner.sdbP.prototype.save AuQXqA9bR7Vk)');
						HTTPUACleaner.logObject(e, true);
					}
				);
			}
		}
		catch (e)
		{
			HTTPUACleaner.logObject(e, true);
		}

		// --------------------

		var promise = file.write(t.encoder.encode(toSaveStr));
		
		promise.then
		(
			function onSuccess(array)
			{
				try
				{
					file.close();
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);
				}

				if (callback)
				try
				{
					callback(true);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}

				return true;
			},
			
			function onFailure(reason)
			{
				/*if (reason instanceof OS.File.Error && reason.becauseNoSuchFile)
				{
					
				}*/

				try
				{
					console.error('------------------------------------------------------------------------------------------------------------------');
					console.error('HUAC: Fatal error in save cert settings');
					console.error(reason);
					
					file.close();
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);
				}

				if (callback)
				try
				{
					callback(false);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}
			}
		).catch
		(
			function(e)
			{
				HTTPUACleaner.logObject(e, true);
			
				try
				{
					file.close();
				}
				catch (e)
				{
					console.error(e);
				}
				
				if (callback)
				try
				{
					callback(false);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}
			}
		);
	};
	
	promise.then
	(
		function onSuccess(file)
		{
			write(file);
			return true;
		},
		
		function onFailure(reason)
		{
			/*if (reason instanceof OS.File.Error && reason.becauseNoSuchFile)
			{
				
			}*/
			try
			{
				console.error('------------------------------------------------------------------------------------------------------------------');
				console.error('HUAC: error in save cert settings');
				console.error(reason);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
			
			if (callback)
				try
				{
					callback(false);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}
		}
	).catch
	(
		function(e)
		{
			HTTPUACleaner.logMessage('HUAC FATAL ERROR: Save certs settings failed: call load error', true);
			HTTPUACleaner.logObject(e, true);

			if (callback)
			try
			{
				callback(false);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true, true);
			}
		}
	);
};

HTTPUACleaner.certs.prototype.loadSettings = function(callback)
{
	var t = this;
	
	var cb = function()
	{
		this.loadSettings(callback);
	};

	if (!this.fileMutex.enter(cb, this))
		return;

	this.load
	(
		function(result, str)
		{
			try
			{
				if (str && str.format == 'mAkxafLsh2da' && str.data)
				{
					t.options = str;
				}
				else
				{
					str = {format: 'mAkxafLsh2da', data: {}};
					t.options = str;
					t.saveSettings();
				}

				try
				{
					if (callback)
						callback(result, t);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true, true);
			}

			t.fileMutex.release();
		}
	);

	return true;
};

HTTPUACleaner.certs.prototype.load = function(callback)
{
	var promise = OS.File.open(this.fileName, {existing: false, read: true, write: true, append: false, truncate: false});

	var t = this;
	
	// JSON.stringify
	// JSON.parse
	var read = function(file)
	{
		var promise = file.read();
		
		promise.then
		(
			function onSuccess(array)
			{
				if (callback)
				{
					var str = null;
					try
					{
						str = t.decoder.decode(array);
						
						if (str.length <= 0)
							str = t.defaultStr;

						file.close();
						if (!str || str.length <= 0)
							str = null;
						else
							str = JSON.parse(str);
					}
					catch (e)
					{
						callback(false);
						HTTPUACleaner.logObject(e, true, true);
						str = false;
					}

					if (str !== false)
					{
						callback(true, str);
					}
				}
				else
					file.close();

				if (HTTPUACleaner['sdk/preferences/service'].get(HTTPUACleaner_Prefix + 'debug.writeSettingFilePathes', false))
					console.error('HUAC opened "cert" settings file ' + t.fileName);

				return true;
			},
			
			function onFailure(reason)
			{
				/*if (reason instanceof OS.File.Error && reason.becauseNoSuchFile)
				{
					
				}*/

				try
				{
					console.error('------------------------------------------------------------------------------------------------------------------');
					console.error('HUAC: Fatal error in load "cert" settings');
					console.error(t.fileName);
					console.error(reason);
					
					file.close();
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}

				if (callback)
				try
				{
					callback(false);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}
			}
		).catch(console.error);
	};
	
	promise.then
	(
		function onSuccess(file)
		{
			read(file);
			return true;
		},
		
		function onFailure(reason)
		{
			/*if (reason instanceof OS.File.Error && reason.becauseNoSuchFile)
			{
				
			}*/
			try
			{
				console.error('------------------------------------------------------------------------------------------------------------------');
				console.error('HUAC: Fatal error in load "cert" settings');
				console.error(t.fileName);
				console.error(reason);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}

			if (callback)
			try
			{
				callback(false);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true, true);
			}
		}
	).catch
	(
		function(e)
		{
			if (callback)
			try
			{
				callback(false);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true, true);
			}

			HTTPUACleaner.logMessage('HUAC FATAL ERROR: Load certs settings failed: call load error', true);
			HTTPUACleaner.logObject(e, true);
		}
	);
};


HTTPUACleaner.certs.prototype.log = function(TLInfo, pageHost, uriHost, domain, IP, isHPKP)
{
	if (!TLInfo || !TLInfo.huacId)
		return;

	if (!this.initialized)
	{
		console.error('HUAC warning: The certs tracking not initialized. Tracking skipped. ' + pageHost + ' / ' + uriHost);
		return;
	}
	
	if (!this.options || this.options.format != 'mAkxafLsh2da' || !this.options.data)
	{
		console.error('HUAC ERROR: The certs tracking has been incorrect initialized. Tracking skipped. ' + pageHost + ' / ' + uriHost);
		console.error(this.options);
		return;
	}

	var data = this.options.data;
	var now  = Date.now();
	var seriousChanged = false;
	
	if (!data.certs)
		data.certs = {};

	if (!data[TLInfo.huacId])
	{
		this.addCertInDb(TLInfo.huacId, null);
		seriousChanged = true;
	}
	
	if (!data[TLInfo.huacId])
	{
		console.error('HUAC error: no certificate found');
		console.error(TLInfo);
		return;
	}

	data[TLInfo.huacId].lastTime = now;

	if (!this.hostsTO)
	if (!TLInfo.isPrivate || !this.hostsTOPrivate)
	{
		if (!this.options.urls)
			this.options.urls = {urls: {}};

		if (!this.options.urls.domains)
			this.options.urls.domains = {};
		
		if (!this.options.urls.domains[domain])
			this.options.urls.domains[domain] = {};
		if (!this.options.urls.domains[domain][uriHost])
			this.options.urls.domains[domain][uriHost] = {first: now};
		this.options.urls.domains[domain][uriHost].last = now;

		var urls = this.options.urls.urls;
		if (!urls[uriHost])
		{
			urls[uriHost] = {IPs: {}};
			seriousChanged = true;
		}


		if (IP)
		{
			if (!urls[uriHost].IPs[IP])
			{
				urls[uriHost].IPs[IP] = {certs: {}, noRoot: {}};
				seriousChanged = true;
			}

			var IPs = urls[uriHost].IPs[IP];
			if (!IPs.certs[TLInfo.huacId])
			{
				seriousChanged = true;
				IPs.certs[TLInfo.huacId] = {first: now};
			}

			IPs.certs[TLInfo.huacId].last = now;
			IPs.certs[TLInfo.huacId].HPKP = isHPKP.HPKP;
			IPs.certs[TLInfo.huacId].HPKPHeader = isHPKP['Public-Key-Pins'];

			var ln = TLInfo.sCerts.length;
			for (var crtI in TLInfo.sCerts)
			{
				var crt = TLInfo.sCerts[crtI];
				if (!IPs.noRoot[crt.sha2])
					seriousChanged = true;

				if (!data.certs[crt.sha2])
					data.certs[crt.sha2] = crt;
				else
				if (data.certs[crt.sha2].sha256SubjectPublicKeyInfoDigest != crt.sha256SubjectPublicKeyInfoDigest)
				{
					var logMsg = 'HUAC ERROR: in certificates the sha2 hash is equals but PKID not equal';
					console.error(logMsg);
					console.error(data.certs[crt.sha2]);
					console.error(crt);
					HTTPUACleaner.logMessage(logMsg);
					HTTPUACleaner.logObject(data.certs[crt.sha2]);
					HTTPUACleaner.logMessage(crt);
				}

				IPs.noRoot[crt.sha2] = {last: now, i: ln - Number(crtI) - 1, l: ln, huacId: TLInfo.huacId};
			}
		}


		if (!data[TLInfo.huacId].hosts[uriHost])
		{
			data[TLInfo.huacId].hosts[uriHost] = {hosts: {}};
			seriousChanged = true;
		}
		
		data[TLInfo.huacId].hosts[uriHost].lastTime = now;
		if (TLInfo.isPrivate)
		{
			/*if (!data[TLInfo.huacId].hosts[uriHost].lastPrivate)
				seriousChanged = true;*/

			data[TLInfo.huacId].hosts[uriHost].lastPrivate = now;
		}
		else
		{
			data[TLInfo.huacId].hosts[uriHost].lastPublic = now;
		}
		
		
		if (!data[TLInfo.huacId].hosts[uriHost].hosts[pageHost])
		{
			data[TLInfo.huacId].hosts[uriHost].hosts[pageHost] = {};
			seriousChanged = true;
		}
		
		data[TLInfo.huacId].hosts[uriHost].hosts[pageHost].lastTime = now;
		if (TLInfo.isPrivate)
		{
			data[TLInfo.huacId].hosts[uriHost].hosts[pageHost].lastPrivate = now;
		}
		else
		{
			data[TLInfo.huacId].hosts[uriHost].hosts[pageHost].lastPublic = now;
		}
	}

	this.saveSettingDecision(now, seriousChanged);
};


HTTPUACleaner.certs.prototype.addCertInDb = function(huacId, cert)
{
	var data = this.options.data;
	if (data[huacId])
		return;

	var certA = HTTPUACleaner.logger.getCertificateByHuacId(huacId);
	if (!certA)
		return;

	if (!cert)
		cert = certA;

	data[huacId] = {hosts: {}};

	data[huacId].tls  = HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_SSL);
	data[huacId].mail = HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_EMAIL);
	data[huacId].obj  = HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_OBJSIGN);
};

// uriHost
HTTPUACleaner.certs.prototype.rootCertificateEnable = function(domain, uriHost, source, httpChannel, taburl)
{
	if (!this.hostsOpt.autodisable)
		return;

	if (!this.options || !this.options.urls)
	{
		console.error('HUAC skipped (because uninitialized) in autodisable certificates: ' + uriHost);
		return;
	}

	var urls = this.options.urls.urls;
	var data = this.options.data;

	var now  = Date.now();

	var hids = {};
	var addToHids = function(uriHost, finded)
	{
		var ips = urls[uriHost] ? Object.keys(urls[uriHost].IPs) : [];

		for (var IP of ips)
		{
			var huacIds = Object.keys(urls[uriHost].IPs[IP].certs);
			if (huacIds.length <= 0)
				continue;

			for (var huacId of huacIds)
			{
				if (finded)
				if (!hids[huacId] || (finded[huacId] && finded[huacId].length > uriHost.length))
					finded[huacId] = uriHost;

				hids[huacId] = true;
			}
		}
	};

	addToHids(uriHost);

	// Если не нашли надлежащий сертификат, ищем все сертификаты, которые имеют тот же домен второго уровня
	var finded = {};
	var dt1, dt2;
	if (Object.keys(hids).length == 0)
	{
		dt1 = Date.now();
		var d = '.' + domain;
		for (var url in urls)
		{
			if (url.endsWith(d))
			{
				addToHids(url, finded);
			}
		}
		
		dt2 = Date.now();
	}


	// Если не нашли сертификатов вообще, то пишем в лог
	if (!urls[uriHost])
	{
		if (source == 2)
		{
			if (!this.unknownHosts)
				this.unknownHosts = {};

			var eHost = HTTPUACleaner.getHostByURI(httpChannel.URI.spec);
			if (!this.unknownHosts[eHost] || now - this.unknownHosts[eHost] > 10*1000)
			{
				var obj = {type: 'certs autodisable', level: 2, msg: {'source': 'http request', description: 'Unknown host for the certs autodisable function', time: (dt2 - dt1) / 1000}};

				if (Object.keys(finded).length > 0)
				{
					var a = 1;
					for (var f in finded)
					{
						obj.msg[a + ') enabled cert for'] = finded[f];
						a++;
					}
				}

				// Установка более длительной задержки
				if (!this.unknownHosts[eHost])
					this.unknownHosts[eHost] = now;
				else
				{
					this.unknownHosts[eHost] = now + 15*60*1000;
					obj.msg.note = 'Warnings suppressed';
				}

				HTTPUACleaner.loggerB.addToLog(taburl, false, httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined', obj);
			}
		}
	}


	for (var huacId in hids)
	{
		var cert = HTTPUACleaner.logger.getCertificateByHuacId(huacId);
		if (cert === false)
		{
			if (data[huacId].old)
				continue;

			// console.error('HUAC ERROR: certificate not found (rootCertificateEnable) ' + huacId);
			var obj = {type: 'certs autodisable', level: 2, msg: {'source': (source == 2 ? 'http request' : 'content-policy'), description: 'HUAC ERROR: certificate not found (rootCertificateEnable)', huacId: huacId}};

			HTTPUACleaner.loggerB.addToLog(taburl, false, httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined', obj);

			data[huacId].old = Date.now();
			continue;
		}

		if (!this.enabledCerts)
			this.enabledCerts = {};


		// !HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_SSL) && 
		if (!data[huacId].ignore)
		if (!this.enabledCerts[huacId] || !HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_SSL))
		{
			HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_SSL);
			
			/*
			console.error('allow ' + (cert.commonName || cert.windowTitle || cert.organization) + ' for ' + uriHost + ' ' + now + ' / ' + source);
			console.error(this.options.urls);*/
		}
		else
		{
			// console.error('allowed ' + now + ' / ' + source);
		}

		if (!this.enabledCerts[huacId])
			this.enabledCerts[huacId] = {last: now, requests: []};
		else
			this.enabledCerts[huacId].last = now;

		if (source == 2 && httpChannel)
		{
			this.enabledCerts[huacId].requests.push(httpChannel);
		}
	}
};

// Отключает сертификаты, которые оказались включёнными, но не должны были
HTTPUACleaner.certs.prototype.rootCertificateFirstDisable = function()
{
	if (this.rootCertificateFirstDisableCounter === true)
		return;
	if (!this.options)
		return;

	if (this.rootCertificateFirstDisableCounter === false)
	{
		this.rootCertificateFirstDisableCounter = HTTPUACleaner.logger.CertDb.getCerts().getEnumerator();
	}

	var t = Date.now();
	var data  = this.options.data;
	var certs = this.rootCertificateFirstDisableCounter;
	var cnt   = 0;
	while (Date.now() - t < 20)
	{
		cnt++;
		
		if (!certs.hasMoreElements())
		{
			this.rootCertificateFirstDisableCounter = true;
			break;
		}

		var cert = certs.getNext();
		cert = cert.QueryInterface(Ci.nsIX509Cert);
		//var certResult = HTTPUACleaner.logger.getCertificateObject(cert);

		if (cert.certType != 1)
			continue;

		let huacId = cert.serialNumber + '/' + cert.sha1Fingerprint + '/' + cert.sha256Fingerprint;
		if (data[huacId] && data[huacId].ignore)
			continue;

		if (
				(!this.enabledCerts || !this.enabledCerts[huacId])
				&& HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_SSL)
			)
		{
			HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, 0);
		}
	}

};

// Отключает сертификаты, которые были подключены для посещения сайтов
HTTPUACleaner.certs.prototype.rootCertificateDisable = function()
{
	if (!this.hostsOpt.autodisable)
	{
		this.clearInterval();
		return;
	}

	this.rootCertificateFirstDisable();

	if (!this.enabledCerts/* || !this.hostsUrlsTimeout*/)
		return;

	if (!this.rootCertificateDisableCounter)
		this.rootCertificateDisableCounter = 0;

	var keys = Object.keys(this.enabledCerts);
	if (keys.length <= 0)
		return;

	var data = this.options.data;

	var now = Date.now();
	var h = Math.min(15, keys.length);
	let j = this.rootCertificateDisableCounter;
	let deleteFlag = false;
	for (var i = 0; i < h; i++, j++)
	{
		if (j >= keys.length || j < 0)
		{
			this.rootCertificateDisableCounter = -i;
			j = 0;
		}

		let huacId = keys[j];

		var key = this.enabledCerts[huacId];
		var isPending = function(key)
		{
			// Даём возможность после Content-Policy заполнить запросами запись сертификата
			if (key.requests.length <= 0 && now - key.last < 1000)
				return true;

			if (key.requests.length > 0)
				key.last = now;

			for (var i = 0; i < key.requests.length; i++)
			{
				var ch = key.requests[i];

				// Провал проверки HPKP
				if (ch.status == 0x805A1FF3 || ch.status == 0x805A4000)
				{
					key.requests.splice(i, 1);
					i--;

					HTTPUACleaner.deleteRequestFromArray();

					continue;
				}

				// https://developer.mozilla.org/en-US/docs/Mozilla/Errors
				// NS_BINDING_REDIRECTED - после перенаправления с http на https остаётся такой статус и isPending
				if (!ch.isPending() || ch.status == Cr.NS_BINDING_REDIRECTED || ch.status == Cr.NS_BINDING_ABORTED)
				{
					key.requests.splice(i, 1);
					i--;
				}
				else
				{/*
					if (now - key.last > 15000)
					{
						console.error(ch.URI.spec);
						console.error(ch.status);
					}*/

					return true;
				}
			}

			return false;
		};

		if (!isPending(key) && now - key.last > this.maxKeepAliveTime)
		{
			try
			{
				var cert = HTTPUACleaner.logger.getCertificateByHuacId(huacId);
				if (!data[huacId].ignore)
					HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, 0);

				delete this.enabledCerts[huacId];
				deleteFlag = true;
				/*
				console.error('disallow ' + (cert.commonName || cert.windowTitle || cert.organization) + ' ' + now);
				console.error(this.enabledCerts);
				console.error(this.options.urls.urls);*/
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
	}
	this.rootCertificateDisableCounter += h;

	if (deleteFlag)
	{
		keys = Object.keys(this.enabledCerts);
		if (keys.length <= 0)
			this.enabledCerts = false;
	}
};

HTTPUACleaner.certs.prototype.isUnknownCertificate = function(regime, TLInfo, pageHost, uriHost, domain, IP, httpChannel, taburl, noRecurse)
{
	if (!this.options || !this.options.urls)
		return undefined;

	if (!TLInfo.sCerts)
		return true;

	var urls = this.options.urls.urls;
	// this.options.urls.domains[domain][uriHost].last
	
	if (this.options.urls.blockedSha2)
	{
		var bs = this.options.urls.blockedSha2;
		for (var crtI = 0; crtI < TLInfo.sCerts.length; crtI++)
		{
			var crtTls = TLInfo.sCerts[crtI];
			if (bs[crtTls.sha2])
			{
				TLInfo.certsFailures                 = {};
				TLInfo.certsFailures.IP              = IP;
				TLInfo.certsFailures.certFounded     = certFounded;
				TLInfo.certsFailures.blockedSha2     = crtTls.sha2;
				TLInfo.certsFailures.ipFound         = false;
				TLInfo.certsFailures.ipFoundOriginal = false;

				var obj = {type: 'certs unknown', level: 3, msg: {'source': 'http response', description: 'sha2 is blocked'}};
				if (regime == 4)
				{
					obj.level = 2;
					obj.msg.description = 'sha2 in black list!';
				}
				HTTPUACleaner.loggerB.addToLog(taburl, false, httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined', obj);

				return true;
			}
		}
	}

	if (!urls[uriHost])
	{
		if (noRecurse === true)
		{
			// Вообще, при нормальной работе сюда попадать ничего не должно
			return true;
		}

		var cntDomain = 0;
		if (this.options.urls.domains && this.options.urls.domains[domain])
		{
			var hosts = this.options.urls.domains[domain];

			var CF = undefined;
			for (var host in hosts)
			{
				cntDomain++;

				delete TLInfo.certsFailures;
				// regime = weak
				var recurseResult = this.isUnknownCertificate(4, TLInfo, pageHost, host, domain, IP, httpChannel, taburl, true);

				if (TLInfo.certsFailures && TLInfo.certsFailures.certFounded)
				if (!CF || TLInfo.certsFailures.certFounded.length > CF.certFounded.length)
					CF = TLInfo.certsFailures;

				// Именно false, а не undefined
				if (recurseResult === false)
				{
					var obj = {type: 'certs unknown', level: 1, msg: {'source': 'http response', description: 'uriHost not found (isUnknownCertificate), but trust is well'}};

					HTTPUACleaner.loggerB.addToLog(taburl, false, httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined', obj);

					delete TLInfo.certsFailures;
					return 0;
				}
			}

			if (regime != 4 && cntDomain > 0)
			{
				TLInfo.certsFailures = CF;
				return true;
			}
		}

		var obj = {type: 'certs unknown', level: 2, msg: {'source': 'http response', description: 'uriHost not found (isUnknownCertificate)'}};

		if (this.unknownHosts[uriHost] <= Date.now())
		{
			HTTPUACleaner.loggerB.addToLog(taburl, false, httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined', obj);
		}

		/*console.error('HUAC ERROR:  isUnknownCertificate: uriHost not found ' + (httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined'));*/

		delete TLInfo.certsFailures;
		return undefined;
	}

	// Если regime == 4, то это режим Weak, то есть действуем аналогично тому, как если бы у нас был новый IP
	var noIP = false;
	if (!IP || IP.startsWith('0.0.0.0:') || IP === '0.0.0.0' || regime == 4)
		noIP = true;

	var ips = [];

	for (var ip in urls[uriHost].IPs)
		ips.push(ip);

	if (!noIP && ips.indexOf(IP) < 0)
		ips.push(IP);


	var IPS = urls[uriHost].IPs;
	var knownIP = !!IPS[IP];

	// IP = null;

	// urls[uriHost].IPs[IP].noRoot[crt.sha2] = {last: now, i: ln - Number(crtI) - 1, huacId: TLInfo.huacId, crt: crt};

	var certFounded = [];
	var certMustFound = [];
	
	for (var crtI in TLInfo.sCerts)
		certMustFound.push(crtI);

	var ipFound = false;
	var ipFoundOriginal = false;
	var isFinded = function(original)
	{
		if (noIP || !knownIP || (!ipFoundOriginal && original))
		{
			for (var i of certMustFound)
				if (certFounded.indexOf(i) < 0)
					if (i != 0)	// Это сертификат хоста, который мы не нашли, потому что он новый (!knownIP) или возможно новый (noIP)
						return false;
			
			return true;
		}

		for (var i of certMustFound)
			if (certFounded.indexOf(i) < 0)
				return false;

		return true;
	};

	var logs = [];
	for (var ip of ips)
	{
		if (IPS[ip] && IPS[ip].noRoot)
		{
			var certsForCheck = {};
			for (var crtH in IPS[ip].noRoot)
			{
				var crt = IPS[ip].noRoot[crtH];
				var hid = crt.huacId ? crt.huacId : '';
				if (!certsForCheck[hid])
					certsForCheck[hid] = {};
				certsForCheck[hid][crtH] = crt;

				ipFound = true;
				if (IP == ip)
				{
					ipFoundOriginal = true;
				}
			}

			// Проходим по всем разным корневым сертификатам
			for (var hid in certsForCheck)
			{
				// Начинаем с корневого сертификата и проверяем цепочку доверия
				for (var crtI = TLInfo.sCerts.length - 1; crtI >= 0; crtI--)
				{
					var fnd    = false;
					var crtTls = TLInfo.sCerts[crtI];

					// Проходим по всем сертификатам, зависящим от выбранного корневого
					for (var crtH in certsForCheck[hid])
					{
						if (crtTls.sha2 == crtH)
						{
							var crt = certsForCheck[hid][crtH];
							if (crtI == TLInfo.sCerts.length - crt.i - 1)
							{
								if (crt.crt)
								{
									if
									(
										   crt.crt.sha2 == crtTls.sha2
										&& crt.crt.name == crtTls.name
										// && crt.crt.num  == crtTls.num	// почему-то серийники могут не совпадать
										&& crt.crt.notAfter == crtTls.notAfter
										&& crt.crt.notBefore == crtTls.notBefore
									)
									{
										if (certFounded.indexOf('' + crtI) < 0)
										{
											certFounded.push('' + crtI);
										}
										fnd = true;
									}
									else
									{
										console.error('HUAC ERROR: certificate is known but has the incorrect inner data');
										console.error(crt.crt);
										console.error(crtTls);
									}
								}
								else
								{
									if (certFounded.indexOf('' + crtI) < 0)
									{
										certFounded.push('' + crtI);
									};
									fnd = true;
								}
							}
							else
							{
								logs.push
								(
									{
										msg: 'HUAC ERROR: certificate is known but has the incorrect level',
										obj: {crtI: crtI, sCerts: TLInfo.sCerts, urls: urls[uriHost], uri: httpChannel.URI.spec, IP: IP}
									}
								);
							}
						}
					}

					if (!fnd)
					{
						break;
					}
				}
			}

			// Если найдена цепочка и сертификат
			if (isFinded(false))
				return false;
		}
	}

	// Если найдена цепочка и сертификат либо найдена цепочка и IP ещё не известен
	if (isFinded(true))
		return false;

	for (var logi in logs)
	{
		if (HTTPUACleaner.debugOptions.IncorrectCertificateLevel)
		{
			console.error(logs[logi].msg);
			console.error(logs[logi].obj);
		}

		HTTPUACleaner.logMessage(logs[logi].msg);
		HTTPUACleaner.logObject(logs[logi].obj);
	}
	
	if (!ipFound)
	{/*
		console.error('certificate is unknown because no have data for urls in HUAC.noRoot');
		console.error(httpChannel.URI.spec);
		console.error(IP);
		console.error(certFounded);
		console.error(urls[uriHost]);
*/
		return undefined;
	}

	TLInfo.certsFailures = {};
	TLInfo.certsFailures.IP = IP;
	TLInfo.certsFailures.certFounded = certFounded;
	TLInfo.certsFailures.ipFound = ipFound;
	TLInfo.certsFailures.ipFoundOriginal = ipFoundOriginal;
	if (IPS[IP] && IPS[IP].noRoot)
		TLInfo.certsFailures.noRoot = IPS[IP].noRoot;

	if (!noRecurse)
	{
		console.error('HUAC Exclamation: certificate is unknown');
		console.error(httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined');
		console.error(IP);
		console.error('' + ipFound + ' ' + ipFoundOriginal);
		console.error(ips);
		console.error(certFounded);
		console.error(urls[uriHost]);
		console.error(TLInfo.sCerts);
	}
	
	return true;
};

HTTPUACleaner.certs.prototype.unknownCerfiticateBlock = function(regime, TLInfo, pageHost, uriHost, domain, IP, httpChannel, taburl)
{
	// 0 или -
	if (regime == 0 || regime == 2)
		return false;

	// +
	if (regime == 1)
		httpChannel.cancel(Cr.NS_ERROR_LOSS_OF_SIGNIFICANT_DATA);

	// Weak и IP-адрес известен
	if (regime == 4 && TLInfo.certsFailures.ipFoundOriginal && TLInfo.certsFailures.certFounded)
	{
		// Если все сертификаты найдены, кроме сертификата сервера,
		// это может означать, что происходит смена сертификатов и на другом IP сертификаты уже сменены,
		// а этот нам попался в первый раз (или что на той же цепочке доверия выписан новый сертификат на данный IP)
		// Если у разных IP полная иерархия сертификатов совпадает, то TLInfo.sCerts.length будет равна TLInfo.certsFailures.certFounded.length
		if (TLInfo.sCerts.length <= TLInfo.certsFailures.certFounded.length + 1)
		{
			// 0 здесь сертификат сервера, в отличие от noRoot, где 0 - корневой сертификат
			// Мы просто смотрим на цепочку доверия без сертификата: нашлась или нет
			var notFound = false;
			for (var i = 1; i < TLInfo.sCerts.length; i++)
				if (TLInfo.certsFailures.certFounded.indexOf(i) < 0)
					notFound = true;

			if (!notFound)
				return false;
		}
	}

	// 1, 3 или 4 - +, Info, Weak
	try
	{
		var obj = {type: 'certs unknown', level: 3, msg: {'source': 'http response', description: 'Certificate unknown'}};
		HTTPUACleaner.loggerB.addToLog(taburl, false, httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined', obj);
	}
	catch (e)
	{
		HTTPUACleaner.logObject(e, true);
	}

	try
	{
		var now = Date.now();
		if (this.options)
		{
			if (!this.options.certsFailure)
				this.options.certsFailure = [];

			var obj    	 = {};
			obj.date   	 = now;
			obj.certs  	 = TLInfo.sCerts;
			obj.priv   	 = TLInfo.isPrivate;
			obj.IP     	 = IP;
			obj.url    	 = httpChannel.URI ? httpChannel.URI.spec : 'httpChannel.URI is undefined';
			obj.regime 	 = regime;
			obj.pageHost = pageHost;
			obj.uriHost  = uriHost;
			obj.taburl   = taburl;
			obj.failures = TLInfo.certsFailures;
			obj.service  = !TLInfo.haveContext;
			obj.regime   = regime;

			if (TLInfo.certsFailures && TLInfo.certsFailures.blockedSha2 && regime != 4)
				httpChannel.cancel(Cr.NS_ERROR_LOSS_OF_SIGNIFICANT_DATA);

			if (
				TLInfo.certsFailures && 
				TLInfo.certsFailures.blockedSha2 &&
				this.options.urls &&
				this.options.urls.blockedSha2 &&
				this.options.urls.blockedSha2[TLInfo.certsFailures.blockedSha2]
				)
			{
				if (!this.blockedSha2)
					this.blockedSha2 = {};

				if (!this.blockedSha2[TLInfo.certsFailures.blockedSha2])
					this.blockedSha2[TLInfo.certsFailures.blockedSha2] = {};

				var so = this.blockedSha2[TLInfo.certsFailures.blockedSha2];
				if (so[pageHost])
				//if (now - so[pageHost] < 8*60*60*1000)
					return regime != 4; // Если не Weak, то мы должны были прекратить запрос

				so[pageHost] = now;
			}

			this.options.certsFailure.push(obj);

			HTTPUACleaner.certificatesFailureTabOpen(httpChannel.isMainDocumentChannel);
		}
	}
	catch (e)
	{
		HTTPUACleaner.logObject(e, true);
	}

	if (!HTTPUACleaner.certsObject.hosts)
	{
		HTTPUACleaner.logMessage('HUAC Excamation: FOR USER: The "Certs" filter enabled but "Tracking TLS certificates (certs.hosts) disabled"', true);
	}
	
	if (regime == 1)
		return true;

	return false;
};
