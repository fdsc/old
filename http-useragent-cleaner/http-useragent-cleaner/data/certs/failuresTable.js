HTTPUACleaner.certs.prototype.showAllFailures = function(worker)
{
	if (worker)
		this.oldFailuresWorker = worker;
	else
		worker = this.oldFailuresWorker;

	if (!worker)
		return false;

	try
	{
		this.unknownHosts = {};
		if (!this.initialized)
		{
			var r = new HTTPUACleaner.html('div');
			r.text(HTTPUACleaner['sdk/l10n'].get('Uninitialized'));
			worker.port.emit("certFailure", {request: ['failures', 'table'], data: r});
			return;
		}

		// var certs = HTTPUACleaner.logger.getCertificates();
		worker.port.emit("certFailure", {request: ['failures', 'table'], data: HTTPUACleaner.certsObject.getFailuresTable()});
		return true;
	}
	catch (e)
	{
		// worker бывает выгружен
		if (e.message != "Couldn't find the worker to receive this message. The script may not be initialized yet, or may already have been unloaded.")
			console.error(e);

		return false;
	}
};

HTTPUACleaner.certs.prototype.eventFailure = function(worker, args)
{
	if (args.request.length > 0 && args.request[0] == 'mousedown')
	{
		if (args.data.id == 'clear')
		{
			this.options.certsFailure = [];

			this.showAllFailures(worker);
			this.saveSettings();
			return;
		}
	}
};

HTTPUACleaner.certs.prototype.getFailuresTable = function()
{
	var urls = HTTPUACleaner.urls;
	var l10n = HTTPUACleaner['sdk/l10n'].get;

	// »наче очень медленно работает
	var strs = 
	{
		'unknown': '',
		'valid until': '',
		'Another sort': '',
		'Date': '',
		'tab url': '',
		'Private': '',
		'Time to live': '',
		'Clear': '',
		'Certificate failures table': '',
		'Number': '',
		'Estimate of strong': '',
		'sign/hash': '',
		'Old certificate': '',
		'Time of live': '',
		'Known': '',
		'Service': '',
		'Date': '',
		'No information': ''
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

	var dv = new html('div', root);
	dv.style.width = '100%';
	var dh = new html('h1', dv);
	dh.text('HUAC: ' + _('Certificate failures table'));
	dh.color('white');
	dh.style.fontWeight = 'bold';
	dh.style.textAlign  = 'center';
	dh.font('Courier New');
	dh.style.fontSize = '2em';


	var setAllButton   = new html('input', root);
	setAllButton.type  = 'button';
	setAllButton.id    = 'clear';
	setAllButton.value = _('Clear');
	setAllButton.data  = {mousedown: true, grayScreen: false, unlock: 'setallignore', 'mousedown-noPreventDefault': true};
	new html('br', root);
	new html('br', root);


	// urls[uriHost].IPs[IP].certs
	var certsTable = new html('table', root);
	certsTable.color(undefined, '#AAAAAA');
	var cnt = 0;
	var t = this;
	var addCertInTable = function(failure)
	{
		cnt++;
		var color = (cnt & 1) > 0 ? '#CCCCCC' : '#DDDDDD';

		var tr = new html('tr', certsTable);
		tr.color('inherited', color);
		var td = new html('td', tr);
		td.text(_('Date'));
		td = new html('td', tr);
		td.text(new Date(failure.date).toLocaleFormat());

		tr = new html('tr', certsTable);
		tr.color('inherited', color);
		td = new html('td', tr);
		td.text(_('tab url'));
		td = new html('td', tr);
		td.text(failure.taburl);
		td.style.maxWidth = '60em';

		tr = new html('tr', certsTable);
		tr.color('inherited', color);
		td = new html('td', tr);
		td.text(_('URL'));
		td = new html('td', tr);
		td.text(failure.url);
		td.style.maxWidth = '60em';

		tr = new html('tr', certsTable);
		tr.color('inherited', color);
		td = new html('td', tr);
		td.text(_('Service'));
		td = new html('td', tr);
		td.text(failure.service);
		
		tr = new html('tr', certsTable);
		tr.color('inherited', color);
		td = new html('td', tr);
		td.text(_('IP'));
		td = new html('td', tr);
		td.text(failure.IP);
		
		tr = new html('tr', certsTable);
		tr.color('inherited', color);
		td = new html('td', tr);
		td.text(_('Private'));
		td = new html('td', tr);
		td.text(failure.priv);
		
		if (failure.failures && failure.failures.blockedSha2)
		{
			tr = new html('tr', certsTable);
			tr.color('inherited', color);
			td = new html('td', tr);
			td.text(_('Blocked sha2'));
			td = new html('td', tr);
			td.text(failure.failures.blockedSha2);
			tr.color(undefined, '#FF0000');
		}

		var cntCert = 0;
		if (failure.certs && failure.certs.length > 0)
		{
			for (var i = failure.certs.length - 1; i >= 0; i--)
			{
				// crt - это новый сертификат
				var crt = failure.certs[i];
				try
				{
					tr = new html('tr', certsTable);
					tr.color('inherited', color);
					td = new html('td', tr);
					td.text(cntCert);
					td.style.fontWeight = 'bold';
					td = new html('td', tr);
					td.text(crt.name);

					tr = new html('tr', certsTable);
					tr.color(undefined, color);
					td = new html('td', tr);
					td.text(_('Known'));
					td = new html('td', tr);
					var knownCert = false;
					if (!failure.failures || failure.failures.blockedSha2)
						td.text('?');
					else
					{
						if (!failure.failures.certFounded || failure.failures.certFounded.indexOf('' + i) < 0)
						{
							td.text(false);
							td.color(undefined, 'red');
						}
						else
						{
							td.text(true);
							knownCert = true;
						}
					}

					tr = new html('tr', certsTable);
					tr.color('inherited', color);
					td = new html('td', tr);
					td.text(_('Time of live'));
					td = new html('td', tr);
					td.text(new Date(crt.notBefore/1000).toLocaleFormat() + ' => ' + new Date(crt.notAfter/1000).toLocaleFormat());

					tr = new html('tr', certsTable);
					tr.color('inherited', color);
					td = new html('td', tr);
					td.text('sha2 256');
					td = new html('td', tr);
					td.text(crt.sha2);
					
					tr = new html('tr', certsTable);
					tr.color('inherited', color);
					td = new html('td', tr);
					td.text(_('Number'));
					td = new html('td', tr);
					td.text(crt.num);
					
					tr = new html('tr', certsTable);
					tr.color('inherited', color);
					td = new html('td', tr);
					td.text(_('Estimate of strong') + ':' + _('sign/hash'));
					td = new html('td', tr);
					td.text(Math.floor(crt.fs*10000)/100 + ' / ' + Math.floor(crt.fh*10000)/100);

					if (failure.failures && failure.failures.noRoot)
					{
						var noRoot = failure.failures.noRoot;
						var crtFinded  = false;
						var crtFindedI = false;
						for (var oldCrtI in noRoot)
						{
							if (noRoot[oldCrtI].i == cntCert)
							{
								crtFinded  = noRoot[oldCrtI];
								crtFindedI = oldCrtI;
								break;
							}
						}

						if (crtFinded)
						{
							tr = new html('tr', certsTable);
							tr.color('inherited', color);
							td = new html('td', tr);
							td.text(_('Old certificate'));
							td = new html('td', tr);
							td.text(crtFindedI);

							if (crt.sha2 != crtFindedI)
								td.color('inherited', 'red');
//{
//	console.error("crtFinded3");
//	console.error(crtFinded.crt);
//}
							if ((!crtFinded.crt || crtFinded.crt.ASN1Structure) && crtFinded.huacId)
							{
								crtFinded.crt = HTTPUACleaner.certsObject.options.data.certs[HTTPUACleaner.logger.getCertificateByHuacId(crtFinded.huacId).sha256Fingerprint];//HTTPUACleaner.logger.getCertificateByHuacId(crtFinded.huacId);
							}
//{
//	console.error("crtFinded2");
//	console.error(crtFinded);
//	console.error(HTTPUACleaner.certsObject.options.data);
//}
							if (crtFinded.crt)
							{
								tr = new html('tr', certsTable);
								tr.color('inherited', color);
								td = new html('td', tr);
								td.text(_('Old certificate'));
								td = new html('td', tr);
								td.text(crtFinded.crt.name);
								
								tr = new html('tr', certsTable);
								tr.color('inherited', color);
								td = new html('td', tr);
								td.text(_('Old certificate'));
								td = new html('td', tr);
								var notBefore = crtFinded.crt.notBefore; // ? crtFinded.crt.notBefore : crtFinded.crt.validity.notBefore;
								var notAfter  = crtFinded.crt.notAfter; //  ? crtFinded.crt.notAfter  : crtFinded.crt.validity.notAfter;
								td.text
								(
									new Date(notBefore/1000).toLocaleFormat() + ' => ' + new Date(notAfter/1000).toLocaleFormat()
								);

								// Подсветка даты старого сертификата, если новый сертификат неизвестен
								if (!knownCert)
								{
									var end  = notAfter/1000;
									var endl = end - notBefore/1000;
									if (Date.now() - end >= 0)
									{
										td.color(undefined, '#00FF00');
									}
									else
									{
										// Берём длительность сертификата, но не более 2-х лет
										/*if (endl > 2*365*24*60*60*1000)
											endl = 2*365*24*60*60*1000;*/

										var enda = (end - Date.now()) / endl * 255;
										if (cntCert == 0)
											enda *= 3;	// Для корневого сертификата считаем срок 1/3 - жёлтый; у них очень большие времена жизни и их могут раньше вывести
										else
											// Для промежуточного - 20%
											// Для серверного - 10%
											enda *= cntCert * 5;

										var endb = 255;
										if (enda > 255)
										{
											endb = 255 - (enda - 255);
											enda = 255;

											if (endb < 0)
												endb = 0;
										}
										if (enda < 0)
											enda = 0;

										enda = Math.floor(enda).toString(16);
										endb = Math.floor(endb).toString(16);
										while (enda.length < 2)
											enda = '0' + enda;
										while (endb.length < 2)
											endb = '0' + endb;

										td.color(undefined, '#' + enda + endb + '00');
									}
								}

								tr = new html('tr', certsTable);
								tr.color('inherited', color);
								td = new html('td', tr);
								td.text(_('Old certificate'));
								td = new html('td', tr);
								td.text(Math.floor(crtFinded.crt.fs*10000)/100 + ' / ' + Math.floor(crtFinded.crt.fh*10000)/100);

								if (crt.fs < crtFinded.crt.fs*0.975 || crt.fh < crtFinded.crt.fh*0.975)
									td.color(undefined, 'red');
								else
								if (crt.fs < crtFinded.crt.fs*0.99 || crt.fh < crtFinded.crt.fh*0.99)
									td.color(undefined, '#FFFF00');
							}
						}
					}
					else
					{
						tr = new html('tr', certsTable);
						tr.color('inherited', color);
						td = new html('td', tr);
						td.text(_('Old certificate'));
						td = new html('td', tr);
						td.text(_('No information'));
					}
				}
				catch (e)
				{
					console.error(e);
				}

				cntCert++;
			}
		}
		else
		{
			tr = new html('tr', certsTable);
			tr.color('inherited', color);
			td = new html('td', tr);
			td.text();
			td = new html('td', tr);
			td.text(_('No information'));
		}
		
		tr = new html('tr', certsTable);
		tr.color('inherited', color);
		td = new html('td', tr);
		td.text(_('Certs regime'));
		td = new html('td', tr);
		if (failure.regime === 0 || failure.regime === 2)
			td.text('-');
		else
		if (failure.regime === 1)
			td.text('+');
		else
		if (failure.regime === 3)
			td.text('Info');
		else
		if (failure.regime === 4)
			td.text('Weak');
		else
			td.text('?');
	};


	var cf = this.options.certsFailure;
	if (cf)
	for (var i = cf.length - 1; i >= 0; i--)
	{
		addCertInTable(cf[i]);

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
