HTTPUACleaner.certs.prototype.showAll = function(worker, noResetCount)
{
	if (worker)
		this.oldCertsWorker = worker;
	else
		worker = this.oldCertsWorker;
	
	if (!worker)
		return;

	try
	{
		this.unknownHosts = {};
		if (!this.initialized)
		{
			var r = new HTTPUACleaner.html('div');
			r.text(HTTPUACleaner['sdk/l10n'].get('Uninitialized'));
			worker.port.emit("cert", {request: ['certs', 'table'], data: r});
			return;
		}

		if (!noResetCount)
		{
			this.resetAll.count = 0;
			this.setAll.count   = 0;
			this.setAllIgnore.count = 0;
		}

		var certs = HTTPUACleaner.logger.getCertificates();
		worker.port.emit("cert", {request: ['certs', 'table'], data: HTTPUACleaner.certsObject.getCertsTable(certs)});
	}
	catch (e)
	{
		// worker бывает уже выгружен
		if (e.message != "Couldn't find the worker to receive this message. The script may not be initialized yet, or may already have been unloaded.")
			console.error(e);
	}
};

HTTPUACleaner.certs.prototype.resetAll = function(worker)
{
	if (!this.resetAll.countReady.bind(this)())
	{
		this.resetAll.count = new Date().getTime();
		this.showAll(worker, true);
		return;
	}
	
	this.resetAll.count = 0;
	var data = this.options.data;
	
	var certs = HTTPUACleaner.logger.CertDb.getCerts().getEnumerator();
	while (certs.hasMoreElements())
	{
		var cert = certs.getNext();
		cert = cert.QueryInterface(Ci.nsIX509Cert);
		/*var certResult = HTTPUACleaner.logger.getCertificateObject(cert);

		if (certResult.usages.nousage)
			continue;
*/

		if (cert.certType != 1)
			continue;
		
		var huacId = cert.serialNumber + '/' + cert.sha1Fingerprint + '/' + cert.sha256Fingerprint;
		if (!data[huacId] || data[huacId].ignore)
			continue;

		HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, 0);
	}

	this.showAll(worker);
};

HTTPUACleaner.certs.prototype.resetAll.count = 0;
// this.resetAll.countReady.bind(this)()
HTTPUACleaner.certs.prototype.resetAll.countReady = function()
{
	return true;/*
	if (new Date().getTime() - this.resetAll.count > 1*60*1000)
		return false;
	
	return true;*/
};

HTTPUACleaner.certs.prototype.setAll = function(worker)
{
	if (!this.setAll.countReady.bind(this)())
	{
		this.setAll.count = new Date().getTime();
		this.showAll(worker, true);
		return;
	}
	
	this.setAll.count = 0;
	
	this.setAllWithFlag(worker, false);
};


HTTPUACleaner.certs.prototype.setAll.count = 0;
// this.setAll.countReady.bind(this)()
HTTPUACleaner.certs.prototype.setAll.countReady = function()
{
	return true;/*
	if (new Date().getTime() - this.setAll.count > 1*60*1000)
		return false;
	
	return true;*/
};

HTTPUACleaner.certs.prototype.setAllIgnore = function(worker)
{
	if (!this.setAllIgnore.countReady.bind(this)())
	{
		this.setAllIgnore.count = new Date().getTime();
		this.showAll(worker, true);
		return;
	}
	
	this.setAllIgnore.count = 0;
	
	this.setAllWithFlag(worker, true);
};


HTTPUACleaner.certs.prototype.setAllIgnore.count = 0;
// this.setAllIgnore.countReady.bind(this)()
HTTPUACleaner.certs.prototype.setAllIgnore.countReady = function()
{
	return true;/*
	if (new Date().getTime() - this.setAllIgnore.count > 1*60*1000)
		return false;
	
	return true;*/
};

HTTPUACleaner.certs.prototype.setAllWithFlag = function(worker, ignoreDefault)
{
	var data = this.options.data;
	var certs = HTTPUACleaner.logger.CertDb.getCerts().getEnumerator();
	while (certs.hasMoreElements())
	{
		var cert = certs.getNext();
		cert = cert.QueryInterface(Ci.nsIX509Cert);
		var certResult = HTTPUACleaner.logger.getCertificateObject(cert);

		if (certResult.certType != 1)
			continue;

		var huacId = cert.serialNumber + '/' + cert.sha1Fingerprint + '/' + cert.sha256Fingerprint;
		if (!data[huacId] || (!ignoreDefault && !data[huacId].tls) || data[huacId].ignore)
			continue;

		HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_SSL);
	}

	this.showAll(worker);
};

HTTPUACleaner.certs.prototype.setAllByDefaultForUninstall = function(worker)
{
	var data = this.options.data;
	var certs = HTTPUACleaner.logger.CertDb.getCerts().getEnumerator();
	while (certs.hasMoreElements())
	{
		var cert = certs.getNext();
		cert = cert.QueryInterface(Ci.nsIX509Cert);
		var certResult = HTTPUACleaner.logger.getCertificateObject(cert);

		if (certResult.certType != 1)
			continue;

		var huacId = cert.serialNumber + '/' + cert.sha1Fingerprint + '/' + cert.sha256Fingerprint;
		if (!data[huacId])
			continue;

		var mask = 0;
		if (data[huacId].tls)
			mask |= HTTPUACleaner.logger.CertDb.TRUSTED_SSL;
		if (data[huacId].mail)
			mask |= HTTPUACleaner.logger.CertDb.TRUSTED_EMAIL;
		if (data[huacId].obj)
			mask |= HTTPUACleaner.logger.CertDb.TRUSTED_OBJSIGN;

		HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, mask);
		this.saveSettings();
	}

	this.showAll(worker);
};


HTTPUACleaner.certs.prototype.setAllDate = function(worker)
{
	var certs = HTTPUACleaner.logger.CertDb.getCerts().getEnumerator();
	var data  = this.options.data;
	while (certs.hasMoreElements())
	{
		var cert = certs.getNext();
		cert = cert.QueryInterface(Ci.nsIX509Cert);
		var certResult = HTTPUACleaner.logger.getCertificateObject(cert);

		if (certResult.certType != 1)
			continue;

		if (!data[certResult.huacId] || !data[certResult.huacId].lastTime || data[certResult.huacId].ignore)
			continue;
		
		HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_SSL);
	}

	this.showAll(worker);
};

HTTPUACleaner.certs.prototype.resetAllDate = function(worker)
{
	var certs = HTTPUACleaner.logger.CertDb.getCerts().getEnumerator();
	var data  = this.options.data;
	while (certs.hasMoreElements())
	{
		var cert = certs.getNext();
		cert = cert.QueryInterface(Ci.nsIX509Cert);
		var certResult = HTTPUACleaner.logger.getCertificateObject(cert);

		if (certResult.certType != 1)
			continue;

		if (data[certResult.huacId] && (data[certResult.huacId].lastTime || data[certResult.huacId].ignore))
			continue;

		HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, 0);
	}

	this.showAll(worker);
};

// Сбрасываем хосты, отображаемые в сертификаты
HTTPUACleaner.certs.prototype.resetAllUrls = function()
{
	console.error('resetAllUrls started');
	console.error(this.options.urls);

	this.options.urls = {urls: {}};

	console.error('reseted all host to certificate history');
};

HTTPUACleaner.certs.prototype.resetAllDataAndUrls = function()
{
	console.error('resetAllDataAndUrls started');
	console.error(this.options.urls);

	this.options.urls = {urls: {}};
	this.options.data = {};

	console.error('resetAllDataAndUrls - ended');
};

HTTPUACleaner.certs.prototype.resetallhistory = function(worker)
{
	var certs = HTTPUACleaner.logger.CertDb.getCerts().getEnumerator();
	var data  = this.options.data;
	while (certs.hasMoreElements())
	{
		var cert = certs.getNext();
		cert = cert.QueryInterface(Ci.nsIX509Cert);
		var certResult = HTTPUACleaner.logger.getCertificateObject(cert);

		if (certResult.certType != 1)
			continue;

		if (!data[certResult.huacId])
			continue;

		data[certResult.huacId].hosts = {};
	}
	
	// На всякий случай сбрасываем сертификаты, которых может уже не быть в БД FireFox, но есть в настройках дополнения
	for (var a in data)
	{
		data[a].hosts = {};
	}

	// this.resetAllUrls();

	this.showAll(worker);
};


HTTPUACleaner.certs.prototype.event = function(worker, args)
{
	if (args.request.length > 0 && args.request[0] == 'mousedown')
	{
		if (args.data.id == 'showfailures')
		{
			HTTPUACleaner.certificatesFailureTabOpen(true);
			return;
		}

		if (args.data.id == 'showhosts')
		{
			HTTPUACleaner.hostsFailureTabOpen();
			return;
		}

		if (args.data.id == 'reset')
		{
			this.resetAll(worker);
			return;
		}
		
		if (args.data.id == 'setall')
		{
			this.setAll(worker);
			return;
		}
		
		if (args.data.id == 'setAllByDefaultForUninstall')
		{
			this.setAllByDefaultForUninstall(worker);
			return;
		}
		
		if (args.data.id == 'setallignore')
		{
			this.setAllIgnore(worker);
			return;
		}

		if (args.data.id == 'setalldate')
		{
			this.setAllDate(worker);
			return;
		}
		
		if (args.data.id == 'resetallnotdate')
		{
			this.resetAllDate(worker);
			return;
		}
		
		if (args.data.id == 'resetallhistory')
		{
			this.resetallhistory(worker);
			this.saveSettings();
			return;
		}

		if (args.data.id == 'resetallurls')
		{
			this.resetAllUrls();
			this.saveSettings();
			return;
		}
		
		if (args.data.id == 'resetAllDataAndUrls')
		{
			this.resetAllDataAndUrls();
			this.saveSettings();
			return;
		}

		if (args.data.id == 'changesort')
		{
			this.sortType++;
			if (this.sortType >= this.sortTypeCount)
				this.sortType = 0;

			this.showAll(worker);
			
			return;
		}
		
		if (args.data.id == 'changeview')
		{
			this.viewType++;
			if (this.viewType >= this.viewTypeCount)
				this.viewType = 0;

			this.showAll(worker);
			
			return;
		}
		
		if (args.data.id == 'autodisable')
		{
			var prefs = HTTPUACleaner['sdk/preferences/service'];
			prefs.set(HTTPUACleaner_Prefix + 'certs.hostsopt.autodisable', !this.hostsOpt.autodisable)

			return;
		}

		/*console.error(args.data.dt.huacId);
		console.error(args.data.id);
		console.error(args.data.checked);*/

		var cert       = HTTPUACleaner.logger.getCertificateByHuacId(args.data.dt.huacId);
		if (cert === false)
		{
			console.error('HUAC ERROR: certificate not found');
			return;
		}
		var certResult = HTTPUACleaner.logger.getCertificateObject(cert);
/*
		certResult.usages.server = HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_SSL);
		certResult.usages.mail   = HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_EMAIL);
		certResult.usages.object = HTTPUACleaner.logger.CertDb.isCertTrusted(cert, cert.certType, HTTPUACleaner.logger.CertDb.TRUSTED_OBJSIGN);
*/
		var fieldName = args.data.id.split('-')[0];
		var state     = !args.data.checked;

		if (fieldName == 'resetHosts')
		{
			var data = this.options.data;
			if (data[certResult.huacId])
				data[certResult.huacId].hosts = {};

			this.saveSettings();
			return;
		}
		
		if (fieldName == 'resetDate')
		{
			var data = this.options.data;
			if (data[certResult.huacId])
				data[certResult.huacId].lastTime = 0;

			this.saveSettings();
			return;
		}

		if (fieldName == 'changeignored')
		{
			var data = this.options.data;
			if (data[certResult.huacId])
				data[certResult.huacId].ignore = state;

			this.saveSettings();
			return;
		}
		
		var mask = 0;
		if (certResult.usages.server)
			mask |= HTTPUACleaner.logger.CertDb.TRUSTED_SSL;
		if (certResult.usages.mail)
			mask |= HTTPUACleaner.logger.CertDb.TRUSTED_EMAIL;
		if (certResult.usages.object)
			mask |= HTTPUACleaner.logger.CertDb.TRUSTED_OBJSIGN;
		
		if (fieldName == 'server')
		{
			if (state)
				mask |= HTTPUACleaner.logger.CertDb.TRUSTED_SSL;
			else
				mask &= ~HTTPUACleaner.logger.CertDb.TRUSTED_SSL;
		}
		else
		if (fieldName == 'mail')
		{
			if (state)
				mask |= HTTPUACleaner.logger.CertDb.TRUSTED_EMAIL;
			else
				mask &= ~HTTPUACleaner.logger.CertDb.TRUSTED_EMAIL;
		}
		else
		if (fieldName == 'object')
		{
			if (state)
				mask |= HTTPUACleaner.logger.CertDb.TRUSTED_OBJSIGN;
			else
				mask &= ~HTTPUACleaner.logger.CertDb.TRUSTED_OBJSIGN;
		}

		HTTPUACleaner.logger.CertDb.setCertTrust(cert, cert.certType, mask);
		this.saveSettings();
	}
};

HTTPUACleaner.certs.prototype.getCertsTable = function(certs)
{
	var urls = HTTPUACleaner.urls;
	var l10n = HTTPUACleaner['sdk/l10n'].get;

	// Иначе очень медленно работает
	var strs = 
	{
		'Signature': '',
		'Signature hash': '',
		'Signature and signature hash': '',
		'Symmetric cipher key length': '',
		'Symmetric cipher': '',
		'Hash for symmetric cryptography': '',
		'Key exchange': '',
		'TLS Version': '',
		'NO': '',
		'YES': '',
		'Truncated': '',
		'records': '',
		'Title': '',
		'Count of certificates': '',
		'Count of certificates in use': '',
		'Count of certificates in not use': '',
		'Count of server certificates': '',
		'Count of not server certificates': '',
		'Count of certificates with the last use date setted': '',
		'Usage': '',
		'nousage': '',
		'server': '',
		'Algorithm': '',
		'mail': '',
		'object': '',
		'issuerName': '',
		'Set all to untrusted': '',
		'Press again to confirm': '',
		'Set all to trusted': '',
		'Set all to trusted with the default ignore': '',
		'Change sort': 'Сменить сортировку',
		'Autodisable certs': '',
		'Last use': '',
		'Never': '',
		'Sites': '',
		'Private': '',
		'Not private': '',
		'Each': '',
		'Another sort': '',
		'Another filter': '',
		'Set all to trusted if the date is setted': '',
		'Set all to untrusted if the date is not setted': '',
		'Set all to default for extension uninstall': '',
		'Reset the host to certificate map': '',
		'All': '',
		'Trusted': '',
		'Trusted and with last use date': '',
		'With last use date': '',
		'Estimated strong': '',
		'Last use date': '',
		'ignored': '',
		'not ignored': '',
		'Ignoring': '',
		'Reset all host history': '',
		'default': '',
		'Trusted, issuer name, certificate name': '',
		'Issuer name, certificate name': '',
		'Certificate name': ''
	};
	
	for (var stra in strs)
	{
		strs[stra] = l10n(stra);
	}
	
	var _  = function(str)
	{
		if (strs[str])
			return strs[str];
		
		return str;
	};
	
	var html = HTTPUACleaner.html;
	

	var root = new html('div');

	// Кнопка сброса истории хостов для всех сертификатов
	new html('br', root);
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	//setAllButton.id    = 'resetallhistory-locker';
	setAllButton.value = '+';
	setAllButton.data  = {mousedown: true, grayScreen: false, unlock: 'resetallhistory', 'mousedown-noPreventDefault': true};
	
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'resetallhistory';
	setAllButton.disabled = true;
	setAllButton.value = _('Reset all host history');
	setAllButton.data  = {mousedown: true, grayScreen: true};
	
	// Кнопка сброса истории хостов, отображаемых в сертификаты
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	//setAllButton.id    = 'resetallurls-locker';
	setAllButton.value = '+';
	setAllButton.data  = {mousedown: true, grayScreen: false, unlock: 'resetallurls', 'mousedown-noPreventDefault': true};

	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'resetallurls';
	setAllButton.disabled = true;
	setAllButton.value = _('Reset the host to certificate map');
	setAllButton.data  = {mousedown: true, grayScreen: false, 'mousedown-noPreventDefault': true};

	// Кнопка сброса все истории
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	//setAllButton.id    = 'resetAllDataAndUrls-locker';
	setAllButton.value = '+';
	setAllButton.data  = {mousedown: true, grayScreen: false, unlock: 'resetAllDataAndUrls', 'mousedown-noPreventDefault': true};

	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'resetAllDataAndUrls';
	setAllButton.disabled = true;
	setAllButton.value = _('Reset the full certs data');
	setAllButton.data  = {mousedown: true, grayScreen: false, 'mousedown-noPreventDefault': true};

	// Кнопка установки всех сертификатов с игнорированием настройки по умолчанию
	new html('br', root);
	
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	//setAllButton.id    = 'setallignore-locker';
	setAllButton.value = '+';
	setAllButton.data  = {mousedown: true, grayScreen: false, unlock: 'setallignore', 'mousedown-noPreventDefault': true};
	
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'setallignore';
	setAllButton.disabled = true;
	setAllButton.value = _('Set all to trusted with the default ignore');
	setAllButton.data  = {mousedown: true, grayScreen: true};

	// Кнопка сброса сертификатов в настройки по умолчанию
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'setAllByDefaultForUninstall-locker';
	setAllButton.value = '+';
	setAllButton.data  = {mousedown: true, grayScreen: false, unlock: 'setAllByDefaultForUninstall', 'mousedown-noPreventDefault': true};
	
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'setAllByDefaultForUninstall';
	setAllButton.disabled = true;
	setAllButton.value = _('Set all to default for extension uninstall');
	setAllButton.data  = {mousedown: true, grayScreen: true};	


	// Кнопка установки всех сертификатов
	new html('br', root);
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'setall';
	setAllButton.value = _('Set all to trusted');
	setAllButton.data  = {mousedown: true, grayScreen: true};

	// Кнопка установки всех сертификатов, у которых есть отметки времени
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'setalldate';
	setAllButton.value = _('Set all to trusted if the date is setted');
	setAllButton.data  = {mousedown: true, grayScreen: true};
	
	// Кнопка сброса всех сертификатов
	new html('br', root);
	var resetAllButton   = new html('input', root);
	resetAllButton.type  = 'button';
	resetAllButton.id    = 'reset';
	resetAllButton.value = _('Set all to untrusted');
	resetAllButton.data  = {mousedown: true, grayScreen: true};

	// Кнопка сброса всех сертификатов, у которых нет отметки времени
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'resetallnotdate';
	setAllButton.value = _('Set all to untrusted if the date is not setted');
	setAllButton.data  = {mousedown: true, grayScreen: true};

	// Таблица статистики сертификатов
	var stat = new html('table', root);
	new html('tr', stat);
	var td = new html('td', stat.html[0]);
	td.text(_('Count of certificates'));
	var td = new html('td', stat.html[0]);
	td.text(certs.length);

	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'showhosts';
	setAllButton.value = _('Hosts');
	setAllButton.data  = {mousedown: true, grayScreen: false, 'mousedown-noPreventDefault': true};
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'showfailures';
	setAllButton.value = _('Failures');
	setAllButton.data  = {mousedown: true, grayScreen: false, 'mousedown-noPreventDefault': true};

	
	
	// Кнопка переключения режима Autodisable root certificate
	// certs.hostsopt.autodisable
	new html('br', root);
	new html('br', root);
	var autodisableCheckbox     = new html('input', root);
	autodisableCheckbox.type    = 'checkbox';
	autodisableCheckbox.id      = 'autodisable';
	autodisableCheckbox.checked = this.hostsOpt.autodisable;
	autodisableCheckbox.data    = {mousedown: true, grayScreen: false, 'mousedown-noPreventDefault': false};
	var t = new html('span', root);
	t.text(_('Autodisable certs'));

	
		// Осторожно, скопировано из logTLS
		var dateX    = (Date.now() - new Date('07/01/2015').getTime())/(1000*60*60*24*365.25);
		var dateX10  = dateX + 50.0;
		var kDate    = 0;//Math.pow(1/2, dateX  /1.5);
		var kDate10  = 0;//Math.pow(1/2, dateX10/1.5);

		var baseCP  = 74 + dateX/1.5;
		var getP     = HTTPUACleaner.logger.getP.bind(HTTPUACleaner.logger);
		var getR     = HTTPUACleaner.logger.getR.bind(HTTPUACleaner.logger);
		var getEqRSA = HTTPUACleaner.logger.getEqRSA.bind(HTTPUACleaner.logger);

	var count = {isTrusted: 0, isNonTrusted: 0, isWebTrusted: 0, isOnlyNonWebTrusted: 0, dateSetted: 0, risk: 1.0, risk2: 1.0, risks: {}};
	var minrsk  = 1.0;
	var minrsk2 = 1.0;
	var crts = [[], [], []];
	var data = this.options.data;
	for (var cert of certs)
	{/*
		if (!cert.usages.stat && cert.usages.nousage)
			console.error(cert);*/
		/* Это неверные проверки, certType говорит не об этом
		// CERT_EXPIRED 4
		if ((cert.certType & Ci.nsIX509Cert.CERT_EXPIRED) > 0 && cert.usages.nousage)
			continue;
	
		// CERT_NOT_TRUSTED 8
		if ((cert.certType & Ci.nsIX509Cert.CERT_NOT_TRUSTED) > 0 && cert.usages.nousage)
			continue;
	
		// CERT_REVOKED 2
		if ((cert.certType & Ci.nsIX509Cert.CERT_REVOKED) > 0 && cert.usages.nousage)
			continue;
*/

		if ((cert.certType & 1) <= 0)
			continue;

		if (!data[cert.huacId])
			this.addCertInDb(cert.huacId, cert.cert);
		
		if ((this.viewType == 1 || this.viewType == 2) && !cert.usages.nousage)
			crts[2].push(cert);
		else
		if (this.viewType == 2 && 
					(data[cert.huacId] && (data[cert.huacId].lastTime || Object.getOwnPropertyNames(data[cert.huacId].hosts).length > 0)))
			crts[2].push(cert);
		else
		if (this.viewType == 3 &&
					(data[cert.huacId] && (data[cert.huacId].lastTime || Object.getOwnPropertyNames(data[cert.huacId].hosts).length > 0)))
			crts[2].push(cert);
		else
		if (this.viewType == 0)
			crts[2].push(cert);

		// Ниже копия
		if ((data[cert.huacId] && data[cert.huacId].lastTime))
			count.dateSetted++;

		// Это тоже копия из logTLS
		var crt= cert.cert;
		var BitsToCorrect = 0;
		var issueDate = Date.now() - crt.validity.notBefore/1000;
		issueDate     = issueDate / (1000*60*60*24*365.25);
		BitsToCorrect = HTTPUACleaner.logger.getBitsToCorrect(issueDate);

		var rsk = 1.0;
		if (cert.algn == 'RSA')
		{
			var p = getP(getEqRSA(cert.algl)-BitsToCorrect, baseCP);

			//p = p + 0.035; // Здесь мы хотим получить оценку, близкую к 100%, а реальные цифры 96.5, 95.3
			if (p < 1.0)
				if (p < 0)
				{
					rsk *= 0.01;
				}
				else
				{
					rsk *= p;
				}
		}
		else
		if (cert.algn == 'ECC')
		{
			var p = getP(getR(256, 135, 384, 200, cert.algl)-BitsToCorrect, baseCP);
			if (p < 1.0)
			{
				if (p < 0)
				{
					rsk *= 0.01;
				}
				else
				{
					rsk *= p;
				}
			}
		}
		else
		{
			rsk *= 0.01;
		}
		
		count.risks[cert.huacId] = {rsk: rsk, date: (new Date(crt.validity.notBefore/1000).toLocaleDateString())};
		

		if ((data[cert.huacId] && data[cert.huacId].lastTime))
		{
			count.risk2 *= 0.995;
			if (minrsk2 > rsk)
				minrsk2 = rsk;
		}


		if (cert.usages.nousage)
		{
			count.isNonTrusted++;
			continue;
		}

		count.risk *= 0.995;
		count.isTrusted++;
		
		if (minrsk > rsk)
			minrsk = rsk;

		if (cert.usages.server)
		{
			count.isWebTrusted++;
			continue;
		}

		count.isOnlyNonWebTrusted++;
	}
	
	count.risk  *= minrsk;
	count.risk2 *= minrsk2;

	if (count.isTrusted == 0)
		resetAllButton.disabled = true;
	
	var addInStatistics = function(text, value)
	{
		tr = new html('tr', stat);
		td = new html('td', tr);
		td.text(text);

		td = new html('td', tr);
		td.text(value);
	};
	
	addInStatistics(_('Count of certificates in use'), count.isTrusted);
	addInStatistics(_('Count of certificates in not use'), count.isNonTrusted);
	addInStatistics(_('Count of server certificates'), count.isWebTrusted);
	addInStatistics(_('Count of not server certificates'), count.isOnlyNonWebTrusted);
	addInStatistics(_('Count of certificates with the last use date setted'), count.dateSetted);
	addInStatistics(_('Estimated strong') + ', %', Math.floor(count.risk*100) + ' / ' + Math.floor(count.risk2*100));
	new html('hr', root);


	// Кнопка смены режимов сортировки сертификатов
	var SortButton   = new html('input', root);
	SortButton.type  = 'button';
	SortButton.id    = 'changesort';
	SortButton.value = _('Another sort');
	SortButton.data  = {mousedown: true, grayScreen: true};

	var SortButtonCaption = new html('span', root);
	if (this.sortType == 0)
		SortButtonCaption.text(_('Trusted, issuer name, certificate name'));
	else
	if (this.sortType == 1)
		SortButtonCaption.text(_('Issuer name, certificate name'));
	else
	if (this.sortType == 2)
		SortButtonCaption.text(_('Certificate name'));
	else
	if (this.sortType == 3)
		SortButtonCaption.text(_('Last use date'));
	else
		SortButtonCaption.text('');
	
	new html('br', root);
		
	// Кнопка смены режимов отображения сертификатов
	SortButton   = new html('input', root);
	SortButton.type  = 'button';
	SortButton.id    = 'changeview';
	SortButton.value = _('Another filter');
	SortButton.data  = {mousedown: true, grayScreen: true};
	
	SortButtonCaption = new html('span', root);
	if (this.viewType == 0)
		SortButtonCaption.text(_('All'));
	else
	if (this.viewType == 1)
		SortButtonCaption.text(_('Trusted'));
	else
	if (this.viewType == 2)
		SortButtonCaption.text(_('Trusted and with last use date'));
	else
	if (this.viewType == 3)
		SortButtonCaption.text(_('With last use date'));
	else
		SortButtonCaption.text('');


	new html('br', root);
	new html('br', root);

	var _this = this;

	// Таблица сертификатов
	var certsTable = new html('table', root);
	var cnt = 0;
	var addCertInTable = function(cert)
	{
		cnt++;
		var color = (cnt & 1) > 0 ? '#CCCCCC' : '#DDDDDD';
		
		var usage = function()
		{
			tr = new html('tr', certsTable);
			tr.color('inherited', color);
			td = new html('td', tr);
			td.text(_('Usage'));
			td = new html('td', tr);

			var form = new html('form',  td);

			var c = new html('input', form);
			c.id  = 'server-' + cnt;
			c.type = 'checkbox';
			c.checked   = cert['usages'].server;
			c.data      = {huacId: cert.huacId, mousedown: true, 'mousedown-noPreventDefault': true};
			var t = new html('span', form);
			t.text(_('server'));
			
			c = new html('input', form);
			c.id  = 'mail-' + cnt;
			c.type = 'checkbox';
			c.checked   = cert['usages'].mail;
			c.data      = {huacId: cert.huacId, mousedown: true, 'mousedown-noPreventDefault': true};
			t = new html('span', form);
			t.text(_('mail'));
			
			c = new html('input', form);
			c.id  = 'object-' + cnt;
			c.type = 'checkbox';
			c.checked   = cert['usages'].object;
			c.data      = {huacId: cert.huacId, mousedown: true, 'mousedown-noPreventDefault': true};
			t = new html('span', form);
			t.text(_('object'));
		};

		var f = function(nm, name, colorA)
		{
			tr = new html('tr', certsTable);
			tr.color('inherited', colorA ? colorA : color);
			td = new html('td', tr);
			td.text(name);
			td = new html('td', tr);
			td.text(nm, 100);
		};
		
		var certTypeStr = cert['certType'];
		// Всегда выполняется
		if (data[cert.huacId])
			certTypeStr += ' (' + _('default') + ' ' + (data[cert.huacId].tls ? '+' : '-') + '/' + (data[cert.huacId].mail ? '+' : '-') + '/' + (data[cert.huacId].obj ? '+' : '-') + ') '
			+ (data[cert.huacId].ignore ? _('ignored') : _('not ignored'));

		f(cert['issuerName'], _('issuerName'));
		f(cert['name'], _('Title'));
		f(certTypeStr, 'certType', data[cert.huacId].ignore ? '#CC0000' : false);
		var clAlg = false;
		if (Number(count.risks[cert.huacId].rsk) < 0.25)
			clAlg = "#FF0000";
		else
		if (Number(count.risks[cert.huacId].rsk) < 0.5)
			clAlg = "#CC8800";
		else
		if (Number(count.risks[cert.huacId].rsk) < 0.7)
			clAlg = "#CCCC00";

		f(cert['algn'] + ' ' + cert['algl'] /*+ ' / ' + cert['algh']*/ + ', ' + count.risks[cert.huacId].date + ', ' + Math.floor(Number(count.risks[cert.huacId].rsk)*100) + '%', _('Algorithm'), clAlg);
		f(cert['sha2'], 'sha2');
		usage();
		
		var reset = function()
		{
			tr = new html('tr', certsTable);
			td = new html('td', tr);
			td.colSpan = 4;
			c = new html('input', td);
			c.id  = 'resetDate-' + cnt;
			c.type = 'button';
			c.value = _('reset date');
			c.data      = {huacId: cert.huacId, mousedown: true, 'mousedown-noPreventDefault': true};
			
			c = new html('input', td);
			c.id  = 'resetHosts-' + cnt;
			c.type = 'button';
			c.value = _('reset hosts');
			c.data      = {huacId: cert.huacId, mousedown: true, 'mousedown-noPreventDefault': true};

			c = new html('input', td);
			c.id  		= 'changeignored-' + cnt;
			c.type		= 'checkbox';
			c.checked 	= !!data[cert.huacId].ignore;
			c.value 	= _('Ignored');
			c.data     	= {huacId: cert.huacId, mousedown: true, 'mousedown-noPreventDefault': true};
			t = new html('span', td);
			t.text(_('Ignoring'));
		}

		// data[cert.huacId].hosts[uriHost][pageHost]
		if (!data[cert.huacId] || !data[cert.huacId].lastTime)
		{
			f(_('Never'), _('Last use'))
		}
		else
			f(new Date(data[cert.huacId].lastTime).toLocaleString(),  _('Last use'));

		if (!data[cert.huacId] || Object.getOwnPropertyNames(data[cert.huacId].hosts).length == 0)
		{
			reset();
			return;
		}
		
		tr = new html('tr', certsTable);
		tr.color('inherited', color);
		td = new html('td', tr);
		td.text(_('Sites'));
		td = new html('td', tr);

		var hostTable = new html('table', td);
		hostTable.color('inherited', color);
		tr = new html('tr', hostTable);
		td = new html('td', tr);
		td.text(_('Private'));
		td = new html('td', tr);
		td.text(_('Not private'));
		td = new html('td', tr);
		td.text(_('Each'));
		/*td = new html('td', tr);
		td.text(_('Page host'));*/
		td = new html('td', tr);
		td.text(_('Request domain'));

		for (var uriHost in data[cert.huacId].hosts)
		{
			tr = new html('tr', hostTable);
			var uriHostObj = data[cert.huacId].hosts[uriHost];

			td = new html('td', tr);
			td.text(uriHostObj.lastPrivate ? new Date(uriHostObj.lastPrivate).toLocaleDateString() : _('Never'));
			td = new html('td', tr);
			td.text(uriHostObj.lastPublic ? new Date(uriHostObj.lastPublic).toLocaleDateString() : _('Never'));
			td = new html('td', tr);
			td.text(uriHostObj.lastTime ? new Date(uriHostObj.lastTime).toLocaleDateString() : _('Never'));
			td = new html('td', tr);
			td.text(uriHost);
			
			for (var pageHost in uriHostObj.hosts)
			{
				tr = new html('tr', hostTable);
				td = new html('td', tr);
				td = new html('td', tr);
				td.colSpan = 3;
				td.text(pageHost);
			}
		}
		
		reset();
	};


	var strComp = function(str1, str2)
	{
		var s1 = str1.toLowerCase();
		var s2 = str2.toLowerCase();
		
		if (s1 > s2)
			return +1;
		if (s1 < s2)
			return -1;
		
		return 0;
	};

	crts[2].sort
	(
		function(cert1, cert2)
		{
			if (_this.sortType == 3)
			{
				var lt1 = 0;
				if ((data[cert1.huacId] && data[cert1.huacId].lastTime))
					lt1 = data[cert1.huacId].lastTime;

				var lt2 = 0;
				if ((data[cert2.huacId] && data[cert2.huacId].lastTime))
					lt2 = data[cert2.huacId].lastTime;
				
				if (lt2 > lt1)
					return +1;
				if (lt2 < lt1)
					return -1;
			}
			
			if (_this.sortType == 0)
			{
				if (cert1.usages.nousage != cert2.usages.nousage)
				{
					if (cert1.usages.nousage)
						return +1;
					else
						return -1;
				}
			}

			if (_this.sortType == 0 || _this.sortType == 1)
			{
				var a = strComp(cert1.issuerName, cert2.issuerName);
				if (a != 0)
					return a;
			}

			return strComp(cert1.name, cert2.name);
		}
	);


	for (var crt of crts[2])
	{
		addCertInTable(crt);
		
		tr = new html('tr', certsTable);
		td = new html('td', tr);
		td.text('');
		td.color('#000000', '#000000');
		td.style['height'] = '0.1em';
		td.colSpan = 2;
	}

	this.saveSettings();

	return root;
};
