HTTPUACleaner.certs.prototype.showAllHosts = function(worker)
{
	if (worker)
		this.oldHostsWorker = worker;
	else
		worker = this.oldHostsWorker;

	if (!worker)
		return;

	try	
	{
		this.unknownHosts = {};
		if (!this.initialized)
		{
			var r = new HTTPUACleaner.html('div');
			r.text(HTTPUACleaner['sdk/l10n'].get('Uninitialized'));
			worker.port.emit("certHosts", {request: ['hosts', 'table'], data: r});
			return;
		}

		worker.port.emit("certHosts", {request: ['hosts', 'table'], data: HTTPUACleaner.certsObject.getHostsTable()});
	}
	catch (e)
	{
		// worker бывает уже выгружен
		if (e.message != "Couldn't find the worker to receive this message. The script may not be initialized yet, or may already have been unloaded.")
			console.error(e);
	}
};

HTTPUACleaner.certs.prototype.eventHosts = function(worker, args)
{
	if (args.request.length > 0 && args.request[0] == 'mousedown')
	{
		if (args.data.id == 'changesorth')
		{
			this.sortTypeHosts++;
			if (this.sortTypeHosts > 2)
				this.sortTypeHosts = 0;

			this.showAllHosts(worker);
			return;
		}
		
		if (args.data.id.startsWith('alert-'))
		{
			if (!this.options.urls.blockedSha2)
				this.options.urls.blockedSha2 = {};
			// console.error(args);
			
			var sha2 = args.data.dt.sha2;
			if (this.options.urls.blockedSha2[sha2])
				delete this.options.urls.blockedSha2[sha2];
			else
				this.options.urls.blockedSha2[sha2] = true;

			this.showAllHosts(worker);
			return;
		}
		
		if (args.data.id.startsWith('hpkp-'))
		{
			var host = args.data.dt.host;
			this.clearHostHPKP(host);

			return;
		}
	}
};

HTTPUACleaner.certs.prototype.clearHostHPKP = function(host)
{
	if (!host)
	{
		HTTPUACleaner.logMessage('HUAC: HPKP not reseted because host is not set', true);
		return;
	}

	// CfYMOiAhtGchgoWAn9jfyb5SOIf9DlBX1VV4id - это просто случайный хеш
	HTTPUACleaner.HPKPService.setKeyPins(host, false, 0, 1, ['CfYMOiAhtGchgoWAn9jfyb5SOIf9DlBX1VV4id/oSS4=']);
	HTTPUACleaner.logMessage('HUAC: HPKP reseted for ' + host, true);
};

HTTPUACleaner.certs.prototype.getHostsTable_ipCache = {};
HTTPUACleaner.certs.prototype.getHostsTable = function()
{
	var urls = HTTPUACleaner.urls;
	var l10n = HTTPUACleaner['sdk/l10n'].get;

	// »наче очень медленно работает
	var strs = 
	{
		'unknown': '',
		'valid until': '',
		'Another sort': '',
		'Strong': ''
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

	/*
	//  нопка сброса истории всех хостов
	new html('br', root);
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'resethall';
	setAllButton.value = _('Reset all host history');
	setAllButton.data  = {mousedown: true, grayScreen: true};

	new html('br', root);
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'reseth6';
	setAllButton.value = _('Reset host history 6 month');
	setAllButton.data  = {mousedown: true, grayScreen: true};

	new html('br', root);
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'resethh';
	setAllButton.value = _('Reset host history for oldest week');
	setAllButton.data  = {mousedown: true, grayScreen: true};*/
/*
	new html('br', root);
	new html('br', root);
	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'resethdouble';
	setAllButton.value = _('Reset host history doubles');
	setAllButton.data  = {mousedown: true, grayScreen: true};
*/
	new html('hr', root);


	//  нопка смены режимов сортировки хостов

	var SortButton   = new html('input', root);
	SortButton.type  = 'button';
	SortButton.id    = 'changesorth';
	SortButton.value = _('Another sort');
	SortButton.data  = {mousedown: true, grayScreen: true};

	var SortButtonCaption = new html('span', root);
	if (this.sortTypeHosts == 0)
		SortButtonCaption.text(_('host'));
	else
	if (this.sortTypeHosts == 1)
		SortButtonCaption.text(_('date max'));
	else
	if (this.sortTypeHosts == 2)
		SortButtonCaption.text(_('date min'));
	else
		SortButtonCaption.text(_('unknown'));

	new html('br', root);
	new html('br', root);

	var _this = this;
	var urls = this.options.urls.urls;
	var data = this.options.data;

	if (!data.certs)
		data.certs = {};
	
	var hosts = Object.keys(urls);

	let ipCache = this.getHostsTable_ipCache;
	var ipComp = function(str1, str2)
	{
		var s1, s2;

		if (!ipCache[str1])
			s1 = HTTPUACleaner.urls.split(str1.toLowerCase(), ['.', ':'], false, 0, true);
		if (!ipCache[str2])
			s2 = HTTPUACleaner.urls.split(str2.toLowerCase(), ['.', ':'], false, 0, true);

		// При сравнении IPv4 можно преобразовывать в числа, при сравнении IPv6 преобразование в число неверно
		// Хотя, при желании, можно было бы использовать Number.parseInt('FF', 16)
		if (!ipCache[str1])
		for (var i = 0; i < s1.length; i++)
		{
			let a = Number(s1[i]);
			if (!Number.isNaN(a))
				s1[i] = a;
		}

		if (!ipCache[str2])
		for (var i = 0; i < s2.length; i++)
		{
			let a = Number(s2[i]);
			if (!Number.isNaN(a))
				s2[i] = a;
		}

		if (!ipCache[str1])
			ipCache[str1] = s1;
		else
			s1 = ipCache[str1];

		if (!ipCache[str2])
			ipCache[str2] = s2;
		else
			s2 = ipCache[str2];
		
		var L = Math.min(s1.length, s2.length);
		for (var i = 0; i < L; i++)
		{
			if (s1[i] < s2[i])
				return -1;
			if (s1[i] > s2[i])
				return +1;
		}
		
		if (s1.length < s2.length)
			return -1;

		if (s1.length > s2.length)
			return +1;

		return 0;
	};
	
	var strComp = function(str1, str2)
	{
		/*var s1 = str1.toLowerCase();
		var s2 = str2.toLowerCase();
		*/
		var s1 = str1.toLowerCase().split('.');
		var s2 = str2.toLowerCase().split('.');

		var s1length = s1.length;
		var s2length = s2.length;
		while (s1.length > s2.length)
			s1.splice(0, 1);
		while (s1.length < s2.length)
			s2.splice(0, 1);

		var L = s1.length;
		for (var i = L - 2 ; i < L; i++)
		{
			if (s1[i] > s2[i])
				return +1;
			if (s1[i] < s2[i])
				return -1;
		}

		for (var i = L - 2 - 1; i >= 0; i--)
		{
			if (s1[i] > s2[i])
				return +1;
			if (s1[i] < s2[i])
				return -1;
		}

		if (s1length > s2length)
			return +1;
		if (s1length < s2length)
			return -1;

		return 0;
	};

	var now = Date.now();
	var getDateCache = {};
	var getDate = function(uriHost)
	{
		if (getDateCache[uriHost])
			return getDateCache[uriHost];

		var ips = Object.keys(urls[uriHost].IPs);
		var date = {min: now, max: 0};
		for (var IP of ips)
		{
			for (var huacId in urls[uriHost].IPs[IP].certs)
			{
				var last = urls[uriHost].IPs[IP].certs[huacId].last;
				if (date.max < last)
					date.max = last;

				if (date.min > last)
					date.min = last;
			}
		}

		getDateCache[uriHost] = date;
		return date;
	};
	
	var getDateIPCache = {};
	var getDateIP = function(uriHost, IP)
	{
		if (getDateIPCache[uriHost] && getDateIPCache[uriHost][IP])
			return getDateIPCache[uriHost][IP];
		
		if (!getDateIPCache[uriHost])
			getDateIPCache[uriHost] = {};

		var date = {min: now, max: 0};
		var a = urls[uriHost].IPs[IP].certs;
		for (var huacId in a)
		{
			var last = a[huacId].last;
			if (date.max < last)
				date.max = last;

			if (date.min > last)
				date.min = last;
		}

		getDateIPCache[uriHost][IP] = date;
		return date;
	};

	var dateCompIP = function(str1, str2, msg, uriHost)
	{
		// urls[uriHost].IPs[IP].certs[huacId].last
		var d1 = getDateIP(uriHost, str1);
		var d2 = getDateIP(uriHost, str2);

		return d1[msg] - d2[msg];
	};

	var dateComp = function(str1, str2, msg)
	{
		// urls[uriHost].IPs[IP].certs[huacId].last
		var d1 = getDate(str1);
		var d2 = getDate(str2);

		return d1[msg] - d2[msg];
	};
	
	// urls[uriHost].IPs[IP].certs
	var certsTable = new html('table', root);
	var cnt = 0;
	var t = this;
	var addCertInTable = function(uriHost)
	{
		cnt++;
		var color = (cnt & 1) > 0 ? '#CCCCCC' : '#DDDDDD';

		var ips = Object.keys(urls[uriHost].IPs);

		var tr = new html('tr', certsTable);
		var td = new html('td', tr);

		td.rowSpan = ips.length + 1;
		// td.colSpan = 3;
		var uriHostSplitted = uriHost.split('.');
		td.style['max-width'] = '18%';
		var noFirst = false;
		for (var h of uriHostSplitted)
		{
			var tdSpan = new html('section', td);
			tdSpan.style.display = 'inline-block';
			if (noFirst)
				tdSpan.text('.' + h);
			else
			{
				tdSpan.text(h);
				noFirst = true;
			}
		}

		if (t.sortTypeHosts == 0)
			ips.sort(ipComp);
		else
		if (t.sortTypeHosts == 1)
			ips.sort(function(a, b) {return dateCompIP(a, b, 'max', uriHost);});
		else
			ips.sort(function(a, b) {return dateCompIP(a, b, 'min', uriHost);});

		var cntIP = 0;
		for (var IP of ips)
		{
			var IPs = urls[uriHost].IPs[IP];

			tr = new html('tr', certsTable);
			td = new html('td', tr);
			td.text(IP);
			td.font('Courier New');

			td = new html('td', tr);
			if (cntIP > 0)
				td.style.borderTop = 'solid black 2px'

			cntIP++;
			for (var huacId in IPs.certs)
			{
				var cert = HTTPUACleaner.logger.getCertificateByHuacId(huacId);

				var ct = null;
				if (cert === false)
				{
					var s = '';
					if (IPs.noRoot)
					{
						for (var crt in IPs.noRoot)
						{
							if (huacId.indexOf(crt) < 0)
								continue;

							var a = IPs.noRoot[crt];

							if (a.i == 0)
								s = crt;
						}
					}
					if (s)
						td.text(_('unknown') + ' (' + s + ' | ' + huacId + ')');
					else
						td.text(_('unknown') + ' (' + huacId + ')');
				}
				else
				{
					var co   = HTTPUACleaner.logger.getCertificateObject(cert);

					var div = new html('div', td);
					div.text(co.name);
					div.style.fontWeight = 'bold';
					//var div = new html('div', td);
					//div.text(new Date(IPs.certs[huacId].last).toLocaleFormat());
					var div = new html('div', td);
					div.text(co.sha2);
					//div.font('Courier New');

					// Вывод статуса HPKP и его сброс
					ct = IPs.certs[huacId];
					var div = new html('div', td);
					
					c = new html('input', div);
					c.id  = 'hpkp-' + cert.huacId;
					c.type = 'image';
					c.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAABKSURBVChThU8JDgAgCMI+1P+/VQ8wKe1YWzGZBKVLFLByKKR3WV5CRnV9g5lyREZhO0jPxHL2Sx6Sacx64HodnCv2MEScLft8E2iJOEsXe/pHuQAAAABJRU5ErkJggq5CYII=';
					c.data      = {huacId: cert.huacId, host: uriHost, mousedown: true, 'mousedown-noPreventDefault': true};
					c.style.marginRight = '0.5em';

					var hpkpAge = ct.HPKP;
					if (hpkpAge === true)
						hpkpAge = '+';
					else
					if (!hpkpAge || Number.isNaN(Number(hpkpAge)))
						hpkpAge = '-';
					else
						hpkpAge = ct.HPKP;

					var span = new html('span', div);
					span.text('HPKP: ' + hpkpAge);
					// div.style.fontWeight = ct.HPKP ? 'bold' : 'normal';
				}

				for (var crt in IPs.noRoot)
				{
					var cert = IPs.noRoot[crt];

					var certObject = cert.crt;
					if (!certObject)
						certObject = data.certs[crt];

					if (!certObject || (cert.huacId && cert.huacId != huacId))
					{
						if (!certObject)
							delete IPs.noRoot[crt];

						continue;
					}

					var isOldCert = now > new Date(certObject.notAfter/1000).getTime();
					if (isOldCert)
					{
						delete IPs.noRoot[crt];
						if (data.certs[crt])
							delete data.certs[crt];

						continue;
					}

					var div = new html('br', td);

					var div = new html('div', td);
					var span = new html('span', div);
					span.text(cert.i);
					span.style.fontWeight = 'bold';
					new html('span', div).text(': ' + (new Date(cert.last).toLocaleFormat()) + ' (' + _('valid until') + ' ' + (new Date(certObject.notAfter/1000).toLocaleFormat()) + ')');

					div.font('Courier New');
					var div = new html('div', td);
					div.text(certObject.name);

					// Ставим хеш и данные по тому, доверенный он или нет
					var div = new html('div', td);

					c = new html('input', div);
					c.id  = 'alert-' + certObject.sha2 + '/' + cert.huacId;
					c.type = 'image';
					if (t.options.urls.blockedSha2 && t.options.urls.blockedSha2[certObject.sha2])
						c.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAABKSURBVChThU8JDgAgCMI+1P+/VQ8wKe1YWzGZBKVLFLByKKR3WV5CRnV9g5lyREZhO0jPxHL2Sx6Sacx64HodnCv2MEScLft8E2iJOEsXe/pHuQAAAABJRU5ErkJggq5CYII=';
					else
						c.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAABGSURBVChTY7zT3s4AAiphlf/BDCj4r/SfEUQz/v/zh4HxIQuKJAyAFDEy3GXAKgkDTFAaJ6CCAphrsQGQHNgEbIogYgwMAGeEF2jBTcK1AAAAAElFTkSuQmCC';
					c.data      = {huacId: cert.huacId, sha2: certObject.sha2, mousedown: true, 'mousedown-noPreventDefault': true};
					c.style.marginRight = '0.5em';

					new html('span', div).text(certObject.sha2);

					
					if (certObject.fh && certObject.fs)
					var div = new html('div', td);
					div.text(_('Strong') + ' ' + Math.floor(certObject.fh*10000)/100 + '% / ' + Math.floor(certObject.fs*10000)/100 + '%');

					if (ct && ct.HPKPHeader && ct.HPKPHeader.indexOf(certObject.sha256SubjectPublicKeyInfoDigest) >= 0)
					{
						var div = new html('div', td);
						div.text('HPKP: ' + certObject.sha256SubjectPublicKeyInfoDigest);
						// div.style.fontWeight = 'bold';
					}
				}
			}
		}
	};


	hosts.sort
	(
		function(host1, host2)
		{
			if (t.sortTypeHosts == 0)
				return strComp(host1, host2);

			if (t.sortTypeHosts == 1)
				return dateComp(host1, host2, 'max');

			return dateComp(host1, host2, 'min');
		}
	);
	
	if (this.options.urls.blockedSha2)
	{
		tr = new html('tr', certsTable);
		td = new html('td', tr);
		td.text(_('sha2 of alerted certificates'));
		td.colSpan = 3;

		for (var sha2 in this.options.urls.blockedSha2)
		{
			tr = new html('tr', certsTable);
			td = new html('td', tr);
			td.colSpan = 3;

			c = new html('input', td);
			c.id  = 'alert-block-' + sha2;
			c.type = 'image';
			c.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAABKSURBVChThU8JDgAgCMI+1P+/VQ8wKe1YWzGZBKVLFLByKKR3WV5CRnV9g5lyREZhO0jPxHL2Sx6Sacx64HodnCv2MEScLft8E2iJOEsXe/pHuQAAAABJRU5ErkJggq5CYII=';
			c.data      = {sha2: sha2, mousedown: true, 'mousedown-noPreventDefault': true};
			c.style.marginRight = '2em';

			new html('span', td).text(sha2);
		}
	}
	
	tr = new html('tr', certsTable);
	td = new html('td', tr);
	td.text('');
	td.color('#000000', '#000000');
	td.style['height'] = '0.1em';
	td.colSpan = 3;

	for (var hostUrl of hosts)
	{
		addCertInTable(hostUrl);

		tr = new html('tr', certsTable);
		td = new html('td', tr);
		td.text('');
		td.color('#000000', '#000000');
		td.style['height'] = '0.1em';
		td.colSpan = 3;
	}

	this.saveSettings();

	return root;
};
