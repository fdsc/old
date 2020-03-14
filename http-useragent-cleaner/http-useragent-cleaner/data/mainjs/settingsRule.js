// FOR THE MOZILLA GALLERY EDITORS
// this.logLevel or other logs variables is not write in console

HTTPUACleaner.sdbP.Rule = function(settings, isTemporary, objectToCopy)
{
	var rules = settings.rules;

	this.enabled  = true;			// Правило включено

	this.condition      = [];		// Условия срабатывания
	this.conditionStr	= '';		// Условия срабатывания в виде строки (то, что ввёл пользователь)
	this.isTabCondition = 0;		// Условие накладывается на url вкладки, или на объект, или на ftype/rtype
	this.isPath			= 0;		// Условие на поддомен, а не на path
	this.port			= '*';		// Условие на порт
	
	this.PRV      = 0;				// Работа везде - 0, в приватном режиме - 1, в обычном режиме - 2
	this.prule    = null;			// Родительское правило, точнее, его номер
	this.crules	  = [];				// Подчинённые правила, точнее, их номера
	this.iconds   = {};				// Индекс подчинённых правил
	this.logLevel = 9;				// Уровень логирования

	this.priority = 5;				// Приоритет правила
	this.filters  = {};				// Фильтры к применению
	this.lastTime = 0;				// Последний раз, когда правило срабатывало
	this.priorityToShow = 5;		// Приоритет для отображения
	this.showChilds	= 1;			// Показывать детей: 1 - да, 0 - нет
	
	// Правило временное - не сохраняется в файл
	this.isTemporary = !!isTemporary;


	// На всякий случай, копирование осуществляется после стандартной инициализации,
	// т.к. при добавлении новых полей загрузка старых файлов настроек может не содержать необходимых полей и они будут неверно инициализированны
	if (objectToCopy)
	{
		for (var name in objectToCopy)
		{
			this[name] = objectToCopy[name];
		}

		for (var fi in this.filters)
		{
			for (var fii = 0; fii < this.filters[fi].length; fii++)
				if (this.filters[fi][fii].val == 0 && !this.filters[fi][fii].log)
				{
					this.filters[fi].splice(fii, 1);
					fii--;
				}

			if (this.filters[fi].length <= 0)
				delete this.filters[fi];
		}

		this.name = '' + this.name;
	}
	else
	{
		this.number   = '' + (++rules.maxNumber);
		this.name     = new String(this.number);
		rules[this.number] = this;
	}

	return this;
};

HTTPUACleaner.sdbP.Rule.prototype.tc = {0: 'url', 1: 'obj'/*, 2: 'ftype', 3: 'rtype'*/};
HTTPUACleaner.sdbP.Rule.prototype.pc = {0: HTTPUACleaner['sdk/l10n'].get('if')}//{0: require("sdk/l10n").get('domain'), 1: require("sdk/l10n").get('path'), 3: '?=', 2: require("sdk/l10n").get('if')};
HTTPUACleaner.sdbP.Rule.prototype.prvTo = {0: 'PN', 1: 'P', 2: 'N'};
HTTPUACleaner.sdbP.Rule.prototype.showChildsStr = {0: '^', 1: '~'};
HTTPUACleaner.sdbP.Rule.prototype.FA = ['ttls', 'rtls', 'context', 'ocsp', 'service', 'cors', 'private', 'frame'];


HTTPUACleaner.sdbP.Rule.prototype.filtersNames = [/*"Fonts", "Plugins", "UA", "Referer", "XForwardedFor", "Storage"*//*, "AcceptHeader"*//*, "Caching", "Etag", */"hCookies", /*"dCookies", "Images", "WebRTC", "WebSocket", 'Fetch', "AJAX", "wname"*//*, "MUA"*//*, "UATI", 'DNT', "NoFilters", "Locale", 'Screen', 'TimeZone', 'Canvas', 'Password', */'Request', 'Log', 'NoLog', 'toHttps', 'iCookies', 'Certs', 'CertsHPKP', 'CacheL', 'CacheV'];

HTTPUACleaner.sdbP.Rule.prototype.filtersLocaled = {};
for (var fName of HTTPUACleaner.sdbP.Rule.prototype.filtersNames)
{
	HTTPUACleaner.sdbP.Rule.prototype.filtersLocaled[fName] = HTTPUACleaner['sdk/l10n'].get(fName);
}

HTTPUACleaner.sdbP.Rule.prototype.filtersStates = 
{
	'Request':  ['0', '+', '-'],
	"hCookies": ['0', '+', '-', 'S'],
	'toHttps':  ['0', '+', '-'],
	'Log':		['0', '+', '-'],
	'NoLog':	['0', '+', '-'],
	'iCookies': ['0', '+', '-', 'Id', 'Ih', 'I1', 'I2', 'I3'],	// В режимах, доходящих в обработчики событий, Id = 2, а не 3, Ih = 3, а не 4, I1 = 1000 и т.п.
	'Certs':	['0', '+', '-', 'Info', 'Weak'],
	'CertsHPKP':['0', '-', 'X', '+S', '+I', '+R', '++S', '++I', '++R'],
	'CacheL':   ['0', '-', 'cache', 'no cache'],
	'CacheV':   ['0', '-', '1', '++', 'NO']
};

HTTPUACleaner.sdbP.Rule.prototype.getEnabledTextAndColor = function()
{
	if (this.isTemporary)
	{
		if (this.enabled)
			return ['@', 'white', '#FF0000'];
		else
			return ['!', 'white', '#995555'];
	}
	else
	{
		if (this.enabled)
			return ['+', 'white', '#000000'];
		else
			return ['X', 'white', '#777777'];
	}
};


HTTPUACleaner.sdbP.Rule.prototype.addChild = function(rule)
{
	this.crules.push(rule.number);
	
	rule.prule = this.number;	
};

HTTPUACleaner.sdbP.Rule.prototype.deleteChild = function(rule)
{
	var i = this.crules.indexOf(rule.number);
	if (i < 0)
	{
		console.error('HUAC error: HTTPUACleaner.sdbP.Rule.deleteChild not found rule ' + rule.number + ' in ' + this.number);
		return;
	}

	rule.prule = null;
	this.crules.splice(i, 1);
};

HTTPUACleaner.sdbP.Rule.prototype.setCondition = function(conditionStr)
{
	this.condition = [];

	if (conditionStr.length <= 0 || conditionStr == '*')
	{
		conditionStr = '';
		this.conditionStr = conditionStr == '*' ? '*' : '';
		return;
	}

	var t = conditionStr.split('|');
	for (var conditionText of t)
	{
		this.setCondition.parseRule.bind(this)(conditionText);
	}

	// console.error(this.condition);
	this.conditionStr = conditionStr;
};

HTTPUACleaner.sdbP.Rule.prototype.setCondition.parseRule = function(text)
{
	text = text.trim();
	if (text.length <= 0)
		return;

	if (text == '*')
		return;


	var t = false;
	var r = this.setCondition.parseRule.split.bind(this)(text, '!=', true);
	if (r.length <= 1)
	{
		r = this.setCondition.parseRule.split.bind(this)(text, '=', true);
		t = true;
	}

	if (r.length <= 1)
	{
		var c = this.setCondition.parseRule.splitted.bind(this)(r[0]);
		this.condition.push(c);
		return;
	}

	if (r.length > 2)
	{
		// Показываем для проверки, что всё плохо
		r[1] = '';
	}

	var r0 = this.setCondition.parseRule.splitted.bind(this)(r[0]);
	var r1 = this.setCondition.parseRule.splitted.bind(this)(r[1]);

	this.condition.push({type: t ? '=' : '!=', r0: r0, r1: r1});
	this.lastTime = 0;
};

HTTPUACleaner.sdbP.Rule.prototype.setCondition.parseRule.split = function(text, splitStr, empty)
{
	var r = text.split(splitStr);
	for (var i = 0; i < r.length; i++)
	{
		r[i] = r[i].trim();
		if (r[i].length <= 0 && !empty)
		{
			r.splice(i, 1);
			i--;
		}
	}
	
	return r;
};

HTTPUACleaner.sdbP.Rule.prototype.setCondition.parseRule.splitted = function(text)
{
	if (text.indexOf('@') == 0)
	{
		return {type: 'regex', text: text};
	}
	
	if (text.indexOf('[') >= 0)
	{
		 var a1 = this.setCondition.parseRule.split.bind(this)(text, '[');
		 var a2 = this.setCondition.parseRule.split.bind(this)(a1[1], ']');
		     a2 = this.setCondition.parseRule.split.bind(this)(a2[0], ':', true);

		if (a2.length == 1)
			a2[1] = a2[0];

		return {type: 'array', name: a1[0].toLowerCase(), index1: a2[0].replace(/[^0-9L]/g, ''), index2: a2[1].replace(/[^0-9L]/g, '')};
	}

	return {type: 'text', text: text};
};

HTTPUACleaner.sdbP.Rule.prototype.preCheckCondition = function(domain, host)
{
	var condition = this.condition;

	if (condition.length <= 0)
		return true;

	var NO = 0;
	var hs = host.split('.').reverse();

	for (var i = 0; i < condition.length; i++)
	{
		var cnd = condition[i];

		// объект сравнения
		switch (this.isTabCondition)
		{
			// Проверяем url
			case 0:
				if (cnd.type == '=' || cnd.type == '!=')
				{
					var r0 = cnd.r0;
					var r1 = cnd.r1;

					// Считаем, что regex срабатывает всегда, т.к. объекта с разложением имени мы не имеем
					if (r0.type == 'regex' || r1.type == 'regex')
						continue;

					if (r1.type == 'array')
					{
						// Два массива, равные друг другу - считаем, что всегда сработает
						if (r0.type == 'array')
							continue;
						
						var a = r0;
						r0 = r1;
						r1 = a;
					}
					
					if (r0.type == 'array')
					{
						// Это что-то не понятное, скорее всего, ошибка
						if (r1.type != 'text')
						{
							continue;
						}

						if (r0.name == 'ct' || r0.name == 'ft' || r0.name == 'cta' || r0.name == 'fta')
							return true;

						if (r0.name == 'td')
						{
							// _@_ - как подстановка
							if (r1.text == '_@_' || r1.text.indexOf('_@_') >= 0)
								return true;

							var rt = r1.text.split('.').reverse();
							
							var fl = true;
							var i1 = r0.index1 == 'L' ? hs.length - 1 : Number(r0.index1);
							var i2 = r0.index2 == 'L' || r0.index2 == '' ? hs.length - 1 : Number(r0.index2);
							try
							{
								for (var ia = i1, j = 0; ia <= i2; ia++, j++)
								{
									if (hs[ia] != rt[j])
										fl = false;
								}
							}
							catch (e)
							{
								// Все правила, где была ошибка, показываем на всякий случай
								// Хотя эти правила, вообще говоря, наоборот, не соответствуют шаблону
								fl = 1;
							}

							if (fl === 1)
								return true;

							if (fl)
							{
								if (cnd.type == '=')
									return true;
								else
									NO++;
							}
							else
							{
								if (cnd.type == '!=')
									return true;
								else
									NO++;
							}
						}
					}
				}

				break;
		}
	}

	// Считаем, что правило всегда сработает, если не нашлось подтверждения иному
	return condition.length > NO;
};

HTTPUACleaner.sdbP.Rule.prototype.checkCondition = function(rulesToExecuted, priority, obj, cnttype, TLInfo, source, turl, rurl, checkObject, errorsMsgs)
{
	if (!this.enabled)
		return false;

	var condition = this.condition;

	if (condition.length <= 0)
		return true;
	
	for (var i = 0; i < condition.length; i++)
	{
		var cnd = condition[i];

		// объект сравнения
		switch (this.isTabCondition)
		{
			// Проверяем url
			case 0:

				if (cnd.type == '=' || cnd.type == '!=')
				{
					var str0 = this.checkCondition.getStr(cnd.r0, checkObject, cnttype, source, TLInfo);
					var str1 = this.checkCondition.getStr(cnd.r1, checkObject, cnttype, source, TLInfo);

					if (str0 === null || str1 === null)
					{
						var cndr0 = cnd.r0.type == 'array' ? cnd.r0.name.toLowerCase() : '';
						var cndr1 = cnd.r1.type == 'array' ? cnd.r1.name.toLowerCase() : '';

						if (source != 'content-policy')
						{
							if (cndr0 == 'cta')
								return str0 === null;
							if (cndr1 == 'cta')
								return str1 === null;
						}

						if (source != 'http response')
						{
							if (cndr0 == 'fta')
								return str0 === null;
							if (cndr1 == 'fta')
								return str1 === null;
						}

						if (source != 'http response')
						{
							if (cndr0 == 'hstatus')
								return str0 === null;
							if (cndr1 == 'hstatus')
								return str1 === null;
						}

						break;
					}


					var f0 = str0 instanceof RegExp;
					var f1 = str1 instanceof RegExp;
					if (f0 || f1)
					{
						if (f0 && f1)
						{
							var estr = 'ERROR in rule: RegExp == RegExp ' + rule.conditionStr;
							console.error(estr);
							errorsMsgs.push(estr);
							break;
						}
						
						if (f1)
						{
							var a = str0;
							str0  = str1;
							str1  = a;
						}
						
						if (str0.test(str1))
						{
							if (cnd.type == '=')
								return true;
						}
						else
							if (cnd.type != '=')
								return true;
						
					}
					else
					if (str0 == str1 && cnd.type == '=')
					{
						return true;
					}
					else
					if (str0 != str1 && cnd.type == '!=')
					{
						return true;
					}
				}
				else
				{
					if (cnd.type == 'text')
					{
						var FA = this.FA;

						var cndtext = cnd.text.toLowerCase();
						if (FA.indexOf(cndtext) >= 0 || (cndtext[0] == '!' && FA.indexOf(cndtext.substr(1)) >= 0))
						{
							var estr = 'HUAC: Error in rule ' + this.name + ' .  Condition type to "obj" ???';
							console.error(estr);
							errorsMsgs.push(estr);
						}
					}

					var estr = 'HUAC: Error in rule ' + this.name + ' .  Condition must be "=" or "!="';
					console.error(estr);
					errorsMsgs.push(estr);
				}
			
				break;
			
			// объект - фрейм, cors, tls
			case 1:

				if (cnd.type == 'text')
				{
					var cndtext = cnd.text.toLowerCase();
					
					if (cndtext == 'ttls' || cndtext == '!ttls' || cndtext == 'rtls' || cndtext == '!rtls')
					{
						var isT  = cndtext == 'ttls' || cndtext == '!ttls';
						var prot = '';
						if (isT)
							prot = checkObject.turl.prot;
						else
							prot = checkObject.rurl.prot;
						
						var isTLS = false;
						if (prot == 'https' || prot == 'wss')
						{
							// TLInfo.f доступно только в ответе от сервера, поэтому в остальных случаях TLInfo.f === true
							if (!isT || isT && (TLInfo.f === true || TLInfo.f*100.0 > TLInfo.minTLSStrong))
								isTLS = true;
						}

						if (isTLS && (cndtext == 'ttls' || cndtext == 'rtls'))
							return true;

						if (!isTLS && (cndtext == '!ttls' || cndtext == '!rtls'))
							return true;
					}
					else
					if (cndtext == 'ocsp' || cndtext == '!ocsp')
					{
						if (TLInfo.isOCSP === null)
							continue;

						if (cndtext == 'ocsp' && TLInfo.isOCSP)
							return true;

						if (cndtext == '!ocsp' && !TLInfo.isOCSP)
							return true;
					}
					else
					if (cndtext == 'context' || cndtext == '!context')
					{
						if (cndtext == 'context' && TLInfo.haveContext)
							return true;

						if (cndtext == '!context' && !TLInfo.haveContext)
							return true;
					}
					else
					if (cndtext == 'service' || cndtext == '!service')
					{
						if (cndtext == 'service' && (!TLInfo.haveContext || TLInfo.isOCSP))
							return true;

						// TLInfo.isOCSP == null на момент request-policy неизвестно, что это именно OCSP
						if (cndtext == '!service' && TLInfo.haveContext && (!TLInfo.isOCSP || TLInfo.isOCSP == null))
							return true;
					}
					else
					if (cndtext == 'frame' || cndtext == '!frame')
					{
						if (!cnttype.ftype)
							break;

						if (cnttype.ftype == 'frame' && cndtext == 'frame')
							return true;
						else
						if (cnttype.ftype != 'frame' && cndtext == '!frame')
							return true;
					}
					else
					if (cndtext == 'cors' || cndtext == '!cors')
					{
						if (cndtext == 'cors' && TLInfo.origin != null)
							return true;
						else
						if (cndtext == '!cors' && TLInfo.origin == null)
							return true;
					}
					else
					if (cndtext == 'private' || cndtext == '!private')
					{
						if (cndtext == 'private' && TLInfo.isPrivate)
							return true;

						if (cndtext == '!private' && !TLInfo.isPrivate)
							return true;
					}
				}
				else
				{
					var estr = 'HUAC: Error in rule ' + this.name + ' . Condition must be a text type';
					console.error(estr);
					errorsMsgs.push(estr);
				}

				break;
		}
	}

	return false;
};

HTTPUACleaner.sdbP.Rule.prototype.checkCondition.getStr = function(cnd, checkObject, cnttype, source, TLInfo)
{
	if (cnd.type == 'array')
	{
		var cndnameLC = cnd.name.toLowerCase();

		if (cndnameLC == 'tprot')
		{
			return checkObject.turl.prot;
		}
		if (cndnameLC == 'rprot')
		{
			return checkObject.rurl.prot;
		}
		
		if (cndnameLC == 'hstatus')
		{
			return TLInfo.hstatus ? TLInfo.hstatus : null;
		}

		if (cndnameLC == 'phase')
		{
			switch (source)
			{
				case 'content-policy':
					return 'CP';
				case 'http response':
					return 'RESP';
				case 'document created':
					return 'DOC';
				case 'http request':
					return 'REQ';
				case 'http request (pre)':
					return 'REQ PRE';
				default:
					console.error('HUAC ERROR: HTTPUACleaner.sdbP.Rule.prototype.checkCondition.getStr have incorrect source parameter: ' + source);
			}

			return '';
		}

		if (cndnameLC == 'ct' || cndnameLC == 'cta')
		{
			if (!cnttype.rtype)
				return null;
			
			if (cnttype.rtype == '_@_')
				return cnttype.rtype;

			if (cnd.index1 == '' && cnd.index2 == '')
			{
				return cnttype.rtype;
			}

			var cntTypeSplit = cnttype.rtype.split(';')[0].split('/');
			var cntTypeResult = '';
			var n1 = cnd.index1 == '' ? 0 : Number(cnd.index1);
			var n2 = cnd.index2 == '' ? 1 : Number(cnd.index2);
			cntTypeResult += cntTypeSplit[n1];

			if (n2 > n1)
			{
				if (cntTypeResult)
					cntTypeResult += '/';

				cntTypeResult += cntTypeSplit[n2];
			}

			return cntTypeResult;
		}
		
		if (cndnameLC == 'ft' || cndnameLC == 'fta')
		{
			if (cnttype.ftype)
				return cnttype.ftype.toUpperCase();

			return null;
		}
		
		var cndName = {'td': ['turl', 'a', 'host'], 'rd': ['rurl', 'a', 'host'], 'tp': ['turl', 'a', 'path'], 'rp': ['rurl', 'a', 'path'], 'tport': ['turl', 'port'], 'rport': ['rurl', 'port']};

		var nm      = cndName[cndnameLC];
		var aStr    = checkObject;

		if (!nm)
		{
			throw new Error('HUAC rule error: condition "' + cnd.name + '" not found');

			return '';
		}

		for (var a of nm)
		{
			aStr    = aStr[a];
		}

		str1 = null;

		var n1, n2;
		if (cnd.index1 == 'L')
		{
			n1 = aStr.length - 1;
		}
		else
			n1 = cnd.index1 == '' ? 0 : Number(cnd.index1);

		if (cnd.index2 == 'L')
		{
			n2 = aStr.length - 1;
		}
		else
			n2 = cnd.index2 == '' ? aStr.length - 1 : Number(cnd.index2);


		var counter = 0;
		if (cndnameLC[1] == 'd')
		{
			for (var i = n2; i >= n1; i--, counter++)
			{
				if (str1 == null)
				{
					if (i >= aStr.length)
						str1 = '_@_';
					else
						str1 = aStr[i];
				}
				else
				{
					if (i >= aStr.length)
						str1 = str1 + '.' + '_@_';
					else
						str1 = str1 + '.' + aStr[i];
				}
				
				if (counter > 10000)
					throw new Error('HUAC Error in rule: index not correct');
			}
		}
		else
		if (cndnameLC[1] == 'p')
		{
			for (var i = n1; i <= n2; i++, counter++)
			{
				if (str1 == null)
				{
					if (i >= aStr.length)
						str1 = '_@_';
					else
						str1 = aStr[i];
				}
				else
				{
					if (i >= aStr.length)
						str1 = str1 + '/' + '_@_';
					else
						str1 = str1 + '/' + aStr[i];
				}
				
				if (counter > 10000)
					throw new Error('HUAC Error in rule: index not correct');
			}
		}

		if (str1 == null)
			str1 = '_@_';

		return str1;
	}
	else
	if (cnd.type == 'text')
		return cnd.text;
	else
	if (cnd.type == 'regex')
	{
		// Для использования в test. Без 'g'
		return new RegExp(cnd.text.substr(1).trim(), '')
	}

	return undefined;
};

HTTPUACleaner.sdbP.Rule.prototype.checkRule = function(rules, rulesToExecuted, priority, obj, cnttype, TLInfo, source, turl, rurl, checkObject, errorsMsgs, pendingFilters, topFilters)
{
	try
	{
		if (!this.checkCondition(rulesToExecuted, priority, obj, cnttype, TLInfo, source, turl, rurl, checkObject, errorsMsgs))
		{
			return;
		}
	}
	catch (e)
	{
		console.error(e);
		console.error('HUAC rule error');
		console.error(this.conditionStr);
		console.error(this);

		errorsMsgs.push('ERROR in rule ' + rule.name);
		return;
	}

	this.lastTime = Date.now();

	for (var filterName in this.filters)
	for (var fIndex in this.filters[filterName])
	{
		var aPr = Array.from(priority);
		aPr.push(Number(this.priority));

		var f = {priority: aPr, filter: filterName, action: this.filters[filterName][fIndex].val, log: this.filters[filterName][fIndex].log, rule: this.name};
		if (this.filters[filterName][fIndex].regime === 0)
		{
			rulesToExecuted.push(f);
			topFilters.push(f);
		}
		else
		{
			if (pendingFilters == null)
				pendingFilters = [];

			pendingFilters.push(f);
		}
	}

	for (var rule of this.crules)
	{
		var aPr = Array.from(priority);
		aPr.push(Number(this.priority));

		rules[rule].checkRule(rules, rulesToExecuted, aPr, obj, cnttype, TLInfo, source, turl, rurl, checkObject, errorsMsgs, pendingFilters ? Array.from(pendingFilters) : null, Array.from(topFilters));
	}


	if (this.crules.length == 0 && pendingFilters)
	{
		var sort = HTTPUACleaner.sdbP.prototype.checkRules.sort;
		/*
		for (var i = 0; i < pendingFilters.length; i++)
		{
			var pf  = pendingFilters[i];

			var find = false;
			for (var j = 0; j < pendingFilters.length; j++)
			{
				if (i == j)
					continue;

				var tf = pendingFilters[j];
				if (pf.filter == tf.filter)
				{
					if (sort(pf, tf) < 0)
					{
						find = true;
						break;
					}
				}
			}

			if (find)
			{
				pendingFilters.splice(i, 1);
				i--;
			}
		}*/
		
		
		var resultFilters = [];
		for (var i = 0; i < pendingFilters.length; i++)
		{
			var pf  = pendingFilters[i];

			var a   = Object.create(pf);
			a.prioritySecondary = pf.priority;

			var aPr = Array.from(priority);
			aPr.push(Number(this.priority));
			aPr.push(-1);
			a.priority = aPr;
			a.erule = this.name;

			rulesToExecuted.push(a);
		}
	}
};
