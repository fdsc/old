
HTTPUACleaner.sdbP.prototype.checkRules = {};

HTTPUACleaner.sdbP.prototype.checkRules.sort = function(a, b)
{
	if (a.priority == b.priority)
		return 0;

	// Проход по всем ячейкам массива приоритетов
	// Чем ниже значение приоритета, тем выше приоритет
	var al = a.priority.length;
	var bl = b.priority.length;
	for (var i = 0; i < al && i < bl; i++)
	{
		// Если приоритет -1, то это значит, что это последняя запись в массиве
		// и она означает, что это фильтр исполнения на листьях
		if (a.priority[i] === -1)
		{
			if (i != al - 1)
				console.error('HUAC ERROR: HTTPUACleaner.sdbP.prototype.checkRules.sort i != a.priority.length - 1');
			break;
		}
		if (b.priority[i] === -1)
		{
			if (i != bl - 1)
				console.error('HUAC ERROR: HTTPUACleaner.sdbP.prototype.checkRules.sort i != b.priority.length - 1');
			break;
		}

		if (a.priority[i] > b.priority[i])
			return -1;
		if (a.priority[i] < b.priority[i])
			return 1;
	}

	var am = a.priority[al - 1] === -1;
	var bm = b.priority[bl - 1] === -1;
	var ac = am ? -1 : 0;
	var bc = bm ? -1 : 0;

	// Если только один из фильтров является исполняющимся на листьях
	if (am ^ bm)
	{
		var c = al + ac - bl - bc;
		if (c == 0)
		{
			if (am)
				return -1;
			else
				return +1;
		}

		return c;
	}
	else
	{
		if (al - bl != 0)
			return al - bl;

		// Если фильтры не являются исполняющимися на листьях
		if (!a.prioritySecondary || !b.prioritySecondary)
		{
			// Т.к. am ^ bm, а каждый фильтр, исполняющийся на листьях, должен иметь prioritySecondary
			// То если хоть один из фильтров не имеет prioritySecondary, это - ошибка
			if (!(!a.prioritySecondary && !b.prioritySecondary))
				console.error('HUAC ERROR: HTTPUACleaner.sdbP.prototype.checkRules.sort !(!a.prioritySecondary && !b.prioritySecondary)');

			return 0;
		}


		// Если оба фильтра исполняются на листьях и их длины равны, значит они либо применены к одному и тому же правилу,
		// либо это совпадение
		// В любом случае, они всё равно имеют один и тот же приоритет и мы их можем отсортировать как нашей душе угодно
		for (var i = 0; i < a.prioritySecondary.length && i < b.prioritySecondary.length; i++)
		{
			if (a.prioritySecondary[i] > b.prioritySecondary[i])
				return -1;
			if (a.prioritySecondary[i] < b.prioritySecondary[i])
				return 1;
		}

		return a.prioritySecondary.length - b.prioritySecondary.length;
	}
};


// HTTPUACleaner.sdb.checkRule.response.bind(HTTPUACleaner.sdb)(httpChannel, obj, cnttype, TLInfo);
HTTPUACleaner.sdbP.prototype.checkRules.response = function(rUrl, obj, cnttype, TLInfo, source)
{
	var msg = {'source': source, 'tab': obj.turl};
	
	if (!this.currentSettings)
	{
		let f = {executed: true, cancel: true, cookie: true, Log: true, NoLog: false, iCookies: 1, certs: 0, CertsHPKP: 0, executedFilters: []};
		f.log = {type: 'side', level: 9, msg: msg};
		return f;
	}

	var rls = this.currentSettings.rules;

	checkObject = {};
	var errorsMsgs  = [];

	var f = this.checkRules.checkUrl.bind(this)(obj, cnttype, TLInfo, source, obj.turl, rUrl, checkObject, errorsMsgs);


	var namename = {'Log': 'Log', 'NoLog': 'NoLog', 'certs': 'Certs', 'cachel': 'CacheL', 'cachev': 'CacheV', 'iCookies': 'iCookies', 'cookie': 'hCookies', 'toHttps': 'toHttps', 'cancel': 'Request', 'CertsHPKP': 'CertsHPKP'};
	var cntOfApplied = 0;
	//for (var filterName in f)
	for (var filterName in namename)
	{
		var filterVal = f[filterName];

		if (!filterVal.log && !filterVal.val || filterVal.log === 2 || filterVal.log === 4)
			continue;

		cntOfApplied++;
		var fNameApplied = '' + cntOfApplied + '>>';
		msg[fNameApplied] = /*logValues[filterVal.log] + */namename[filterName] + ':';

		if (filterName == 'cancel')
			msg[fNameApplied] += filterVal.val ? '+' : '-';
		else
		if (filterName == 'cookie')
		{
			if (!filterVal.val)
				msg[fNameApplied] += '-';
			else
				msg[fNameApplied] += filterVal.val === true ? '+' : 'S';
		}
		else
		if (filterName == 'iCookies')
		{
			if (filterVal.val == 0)
				msg[fNameApplied] += '-';
			else
			if (filterVal.val == 1)
				msg[fNameApplied] += '+';
			else
			if (filterVal.val == 2)
				msg[fNameApplied] += 'Id';
			else
			if (filterVal.val == 3)
				msg[fNameApplied] += 'Ih';
			else
			if (filterVal.val >= 1000)
				msg[fNameApplied] += 'I' + (filterVal.val - 1000 + 1);
			else
				msg[fNameApplied] += '???ERROR???' + filterVal.val;
		}
		else
		if (filterName == 'certs' || filterName == 'certsHPKP'	|| filterName == 'cachel' || filterName == 'cachev')
		{
			msg[fNameApplied] += HTTPUACleaner.sdbP.Rule.prototype.filtersStates[namename[filterName]][filterVal.val];
		}
		else
		if (filterVal.val === true || filterVal.val === false)
			msg[fNameApplied] += filterVal.val ? '+' : '-';
		else
			msg[fNameApplied] += filterVal.val == 1 ? '+' : '-';

		msg[fNameApplied] += filterVal.log ? ' (logged)' : '';
	}

	
	var logValues = ['0', '*', 'x', '+', 'X'];
	for (var i = f.executedFilters.length - 1; i >= 0; i--)
	{
		var fa = f.executedFilters[i];

		var prefix = !fa.log ? '0' : logValues[fa.log];

		if (fa.priority[fa.priority.length - 1] == -1)
			prefix += 'v';
		else
			prefix += 'o';

		msg['' + i] = '[' + prefix + ']' + fa.filter + ': ' + HTTPUACleaner.sdbP.Rule.prototype.filtersStates[fa.filter][fa.action] + ' / ' + fa.rule + ' -> ' + fa.priority;

		if (fa.prioritySecondary)
		{
			msg['' + i] += ' (' + fa.prioritySecondary + ')' + ' [' + fa.erule + ']';
		}
	}

	for (var i = errorsMsgs.length - 1; i >= 0; i--)
	{
		msg['e' + i] = errorsMsgs[i];
	}

	if (errorsMsgs.length > 0)
		f.executed = true;

	var level = 1;

	if (level == 1)
	{
		for (var key in f)
		{
			if (key == 'executedFilters' || key == 'executed')
				continue;

			if (f[key].log == 1 || f[key].log == 3)
			{
				level = 9;
				break;
			}
		}
	}
	
	for (var key in f)
	{
		if (key == 'executedFilters' || key == 'executed')
			continue;

		// Всё остальное дополнение ждёт только лишь val, а не всю запись
		f[key] = f[key].val;
	}

	if (f.NoLog)
	{
		level = 1;
	}

	if (f.Log)
	{
		level = 9;
		msg.parseUrl = JSON.stringify(checkObject);
	}

	if (level == 1 && !f.NoLog)
		// toHttps не логируется специально на 9-ом уровне
		if (f.toHttps && (rUrl.indexOf('http:') == 0 || rUrl.indexOf('ws:') == 0))
			level = 4;

	if (errorsMsgs.length > 0)
		level = 10;

	f.log = {type: 'side', level: level, msg: msg};

	return f;
};


// Проверяем URL, если хоть один фильтр применён - возвращаем executed = true, иначе - executed = false
HTTPUACleaner.sdbP.prototype.checkRules.checkUrl = function(obj, cnttype, TLInfo, source, turl, rurl, checkObject, errorsMsgs)
{
	var result = {cancel: {val: false, log: 0}, cookie: {val: false, log: 0}, Log: {val: false, log: 0}, NoLog: {val: false, log: 0},
				iCookies: {val: 0, log: 0}, toHttps: {val: 0, log: 0}, certs: {val: 0, log: 0}, CertsHPKP: {val: 0, log: 0}, cachel: {val: 0, log: 0}, cachev: {val: 0, log: 0},
				executed: false, executedFilters: []};

	if (!checkObject || !checkObject.turl || !checkObject.rurl)
	{
		this.checkRules.getUrlsObjects.bind(this)(turl, rurl, checkObject);
	}

	var rulesToExecuted = [];
	for (var ruleI in /*this.currentSettings.rules*/this.topLevelRules)
	{
		var rule = this.topLevelRules[ruleI]; //this.currentSettings.rules[ruleI];
		if (rule instanceof HTTPUACleaner.sdbP.Rule && rule.prule == null)
		{
			try
			{
				rule.checkRule(this.currentSettings.rules, rulesToExecuted, '', obj, cnttype, TLInfo, source, turl, rurl, checkObject, errorsMsgs, null, []);
			}
			catch (e)
			{
				console.error(e);
			}
		}
	}

	if (rulesToExecuted.length <= 0)
		return result;

	rulesToExecuted.sort
	(
		this.checkRules.sort
	);

	var setLog = function(obj, val)
	{
		if (val == 0)
			return;

		if (!obj.log || obj.log < 3)
			obj.log = val;
		else
		if (val >= 3)
			obj.log = val;
	}

	for (var i = 0; i < rulesToExecuted.length; i++)
	{
		var f = rulesToExecuted[i];
		//console.error(f);
		
		result.executedFilters.push(f);

		// Добавить выше при добавлении нового фильтра в условие на уровень логирования
		if (f.filter == 'Request')
		{
			if (f.action == 1)
			{
				result.executed   = true;
				result.cancel.val = true;
			}
			else
			if (f.action == 2)
			{
				result.cancel.val = false;
			}

			setLog(result.cancel, f.log);
		}

		if (f.filter == 'CertsHPKP')
		{
			if (f.action > 0)
			{
				result.CertsHPKP.val = f.action == 1 ? 0 : f.action;
			}

			setLog(result.CertsHPKP, f.log);
		}

		if (f.filter == 'Certs')
		{
			if (f.action > 0)
			{
				result.certs.val = f.action;
			}

			setLog(result.certs, f.log);
		}
		
		if (f.filter == 'CacheL')
		{
			if (f.action > 0)
			{
				result.cachel.val = f.action;
			}

			setLog(result.cachel, f.log);
		}

		if (f.filter == 'CacheV')
		{
			if (f.action > 0)
			{
				result.cachev.val = f.action;
			}

			setLog(result.cachev, f.log);
		}

		if (f.filter == 'toHttps')
		{
			if (f.action == 1)
			{
				/*var prot1 = checkObject.turl.prot;
				var prot2 = checkObject.rurl.prot;

				if ((prot1 == 'https' || prot1 == 'wss') && (prot2 == 'https' || prot2 == 'wss'))
				{
					// Ничего не делаем, т.к. ничего делать и не нужно
				}
				else*/
				{
					// Этот фильтр просто так не логируется
					// result.executed = true;
					result.toHttps.val = true;
				}
			}
			else
			if (f.action == 2)
			{
				result.toHttps.val = false;
			}

			setLog(result.toHttps, f.log);
		}

		if (f.filter == 'hCookies')
		{
			if (f.action == 1)
			{
				result.executed   = true;
				result.cookie.val = true;
			}
			else
			if (f.action == 2)
			{
				result.cookie.val = false;
			}
			else
			if (f.action == 3)
			{
				result.executed = true;
				result.cookie.val = 2;
			}
			
			setLog(result.cookie, f.log);
		}
		
		if (f.filter == 'iCookies')
		{
			if (f.action == 1)
			{
				result.executed     = true;
				result.iCookies.val = 1;		// включена изоляция вместе со сбросом (режим "+")
			}
			else
			if (f.action == 2)
			{
				result.iCookies.val = 0;		// фильтр выключен
			}
			else
			if (f.action == 3)
			{
				result.executed     = true;
				result.iCookies.val = 2;		// включена изоляция без сброса, режим Id
			}
			else
			if (f.action == 4)
			{
				result.executed     = true;
				result.iCookies.val = 3;		// включена изоляция без сброса, режим Ih
			}
			else
			if (f.action > 4)
			{
				result.executed     = true;
				result.iCookies.val = 1000 + f.action - 5;		// включена отдельная изоляция
			}

			setLog(result.iCookies, f.log);
		}

		if (f.filter == 'Log' && f.action == 1)
		{
			result.executed = true;
			result.Log.val  = true;
		}

		if (f.filter == 'Log' && f.action == 2)
		{
			result.Log.val  = false;
		}
		
		if (f.filter == 'Log')
			setLog(result.Log, f.log);

		if (f.filter == 'NoLog')
		{
			if (f.action == 1)
			{
				result.NoLog.val = true;
			}
			else
			if (f.action == 2)
			{
				result.NoLog.val = false;
			}

			setLog(result.NoLog, f.log);
		}
	}

	return result;
};


// Парсим URL и готовим объект для того, чтобы дальше парсить строки

HTTPUACleaner.sdbP.prototype.checkRules.getUrlsObject = function(url, checkObject)
{
	var host  = this.urls.getHostByURI(url);
	var prot  = this.urls.getProtocolFromURL(url, true);
	
	// Преобразуем host в массив имён, разделённых точкой
	// Например, mail.yandex.ru должен быть преобразован в ['ru', 'yandex', 'mail']
	var ahost = this.urls.toArrayPoint(host);
	var port  = ahost.splice(0, 1);

	if (port.length == 0)
	{
		if (prot == 'http')
			port = '80';
		else
		if (prot == 'https')
			port = '443';
	}

	var path    = this.urls.getPathByURI(url);
	var pathPrm = path.split('?');
	var apath   = this.urls.toArray(pathPrm[0]);
	var params  = pathPrm[1] ? pathPrm : '';


	// checkObject.url    = url;	// Для отладки
	checkObject.host   = host;
	checkObject.prot   = prot;
	checkObject.port   = port;
	checkObject.path   = path;
	checkObject.params = params;

	checkObject.a      = {};
	checkObject.a.host = ahost;
	checkObject.a.path = apath;
}

HTTPUACleaner.sdbP.prototype.checkRules.getUrlsObjects = function(turl, rurl, checkObject)
{
	checkObject.turl = {};
	checkObject.rurl = {};

	this.checkRules.getUrlsObject.bind(this)(turl, checkObject.turl);
	this.checkRules.getUrlsObject.bind(this)(rurl, checkObject.rurl);
// console.error(checkObject);
};
