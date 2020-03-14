// Ниже HTTPUACleaner.html

HTTPUACleaner.sdbP.prototype.getHtmlTab_MainCondition = function(settings, rule, url, td)
{
	var create = HTTPUACleaner.html;

	var text   = new create('input', td);
	text.type  = 'text';

	text.id	   = 'rulefilters-maincond-' + rule.number;
	text.value = rule.conditionStr;
	
	text.data = {};
	text.data['mousedown-noPreventDefault'] = true;

	return text;
};

HTTPUACleaner.sdbP.prototype.getHtmlTab_Filters = function(settings, rule, td)
{
	var setData = function(td, type, filterName, fIndex)
	{
		td.data = {};
		td.data.number = rule.number;
		td.data.type   = 'rule';
		td.data.etype  = 'filter-' + type;
		td.id          = td.data.type + td.data.etype + rule.number;
		td.data.fName  = filterName;
		td.data.fIndex = fIndex;
		td.data.mousedown  = true;
	};
	
	var create = HTTPUACleaner.html;
	
	var html = new create('div', td);

	for (var fiName in rule.filters)
	for (var fIndex in rule.filters[fiName])
	{
		var fName = rule.filtersLocaled[fiName];
		new create('span', html).text(' ');
		var sall = new create('span', html);
		var sl   = new create('span', sall);
		var so   = new create('span', sall);
		var s1   = new create('span', sall);
		var s2   = new create('span', sall);
		
		sall.style['white-space'] = 'nowrap';

		s1.text(fName + ':' + rule.filtersStates[fiName][rule.filters[fiName][fIndex].val]);

		setData(s1, 'filter', fiName, fIndex);
		s1.data.value = rule.filters[fiName][fIndex].val;
		s1.style['margin-right'] = '2em';
		
		var si = new create('img', so);
		so.style.marginRight = '2px';
		setData(so, 'filterregime', fiName, fIndex);

		if (rule.filters[fiName][fIndex].regime == 0)
		{
			si.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAABGSURBVChTY7zT3s4AAiphlf/BDCj4r/SfEUQz/v/zh4HxIQuKJAyAFDEy3GXAKgkDTFAaJ6CCAphrsQGQHNgEbIogYgwMAGeEF2jBTcK1AAAAAElFTkSuQmCC';
			so.style.verticalAlign = '+0px';
		}
		else
		{
			si.src ='data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAkAAAAJCAYAAADgkQYQAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAC1SURBVChTY3wro/yfgYGRARf4D5Rl+s/AtAfMwgpA+v8fZ7wvL8/P94f1FdAwNqgMEvj/h4n5jzKTwoMHH4HqJwMN+wiVAQOo4UsFHjx4xPgfynsnq/oOSAmCOSDwn+Gr0FNpYYb/B34yQYUYmP4xZgA1/Ibw/v9kZPpXClIA4sFNAoG3sipXGP8zaP9nYHwo/OS2AlSYAW4SCHD8ZI8A6WNgYUqDCkEAyCRk/FRSTRVV7D8DAGAVWih0AbDwAAAAAElFTkSuQmCC';
			so.style.verticalAlign = '+0px';
		}

		si = new create('img', sl);
		sl.style.marginRight = '2px';
		setData(sl, 'filterlog', fiName, fIndex);
		if (rule.filters[fiName][fIndex].log)
		{
			if (rule.filters[fiName][fIndex].log === 1)
			{
				si.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAkAAAAJCAYAAADgkQYQAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAABTSURBVChTY/j//z8Yf+udCKT+M7yVUQHTMD4IY1WArhBFB7oiEAbJM4IY72RVgcohQOjxbUZ0PooubBgkT5xJxLgJzgDR6Ipg4nC70RUibPjPAACXI8l8FEyG7AAAAABJRU5ErkJggg==';
				so.style.verticalAlign = '+0px';
			}
			else
			if (rule.filters[fiName][fIndex].log === 2)
			{
				si.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAABESURBVChThY8BCgAgCANnX9Rn+kdj0CIKaaCTQyaauxeWMtPoEbEZqgpcop8l9oB7NjZJ0TpFjeW9FNWdeIBK7PMmMAG63mprJnWe5wAAAABJRU5ErkJggg==';
				so.style.verticalAlign = '+0px';
			}
			else
			if (rule.filters[fiName][fIndex].log === 3)
			{
				si.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAtSURBVChTY/z//z8DHDAyQjj//zOCaSBggtI4AUEFIDOR7MAERJhAY0cyMAAARF0OBkv+jm0AAAAASUVORK5CYIL8ndkGFxYeDNJApT4AAAAASUVORK5CYIIAAElFTkSuQmCCeCRb0z8KBkqMV0ZXwiWcw5FDhqcF5mzjkKBBB+wArpzz4NObFfKj/V/gBm3b/OtPv1dEKrp2yNQFThxa+Bm6K6X6Dbwua3/zzCxYAAAAAElFTkSuQmCC';
				so.style.verticalAlign = '+0px';
			}
			else
			if (rule.filters[fiName][fIndex].log === 4)
			{
				si.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAABKSURBVChThU8JDgAgCMI+1P+/VQ8wKe1YWzGZBKVLFLByKKR3WV5CRnV9g5lyREZhO0jPxHL2Sx6Sacx64HodnCv2MEScLft8E2iJOEsXe/pHuQAAAABJRU5ErkJggq5CYII=';
				so.style.verticalAlign = '+0px';
			}
		}
		else
		{
			si.src ='data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAACTSURBVChTY/z//z8DCDg4OPBwcnJuArHZ2dmDNmzY8AHEZgIRIMDBwVHAyMh4A6jhws+fP6ugwggFTExMZn///t0GZIJMMQELAgFcwb9//yzY2NhOsLKyXgGaogMVRijABZCtOPHr1y+L379/6wDdcgUqjFAAchxQwgmIvUBsqDBCAdBrfUBJC6CkA9AdbRBRBgYAc6QyTeoctnAAAAAASUVORK5CYIIK/n1TCvEybbsAh4y9eZIsCSHkC8ACWfKDE/rcAAAAAElFTkSuQmCC';
			so.style.verticalAlign = '+0px';
		}
	}
	
	return html;
};

HTTPUACleaner.sdbP.prototype.getHtmlTab = function(SideLogURL, settings, scrool, lastUrls)
{
	if (!scrool)
		scrool = 0;

	this.lastSideLogURL = SideLogURL;
	this.lastUrls       = lastUrls;

	var _ = HTTPUACleaner['sdk/l10n'].get;
	var urls = HTTPUACleaner.urls; //require('./getURL');

	var domain = SideLogURL; // ? urls.getDomainByHost(urls.getHostByURI('http://' + SideLogURL)) : null;

	// rules - все правила
	// marks - обрабатываемые правила
	var FA = HTTPUACleaner.sdbP.Rule.prototype.FA;
	var isErrorInConditionRecursive = function(rules, marks)
	{
		var getRule = function(rule)
		{
			if (rule instanceof HTTPUACleaner.sdbP.Rule)
				return rule;

			return rules[rule];
		};

		// Проверка на то, что условие верного типа
		// Если условие на url
		var isErrorCondition = function(mark)
		{
			var cnd = mark.condition;

			for (var sCnd of cnd)
			{
				// Если условие на объект, то тип должен быть текстовый
				if (mark.isTabCondition === 1)
				{
					if (sCnd.type !== 'text')
						return true;

					var cndtext = sCnd.text.toLowerCase();

					if (FA.indexOf(cndtext) < 0 && (cndtext[0] != '!' || FA.indexOf(cndtext.substr(1)) < 0))
						return true;
				}

				 // Если условие на url, то это условие должно быть условием равенства или неравенства
				if (mark.isTabCondition === 0 && (sCnd.type !== '=' && sCnd.type !== '!=')) 
					return true;

				if (mark.isTabCondition === 0)
				{
					if (sCnd.r1 && sCnd.r1.type == 'text')
					{
						var txt = sCnd.r1.text;

						// Если вторая часть сравнения начинается с "=", возможно, кто-то неверно написал сравнение с '==' вместо '='
						// Либо если вторая часть вообще пустая, что явно некорректно
						if (!txt || txt.startsWith('=') || txt.length <= 0)
						{
							return true;
						}
					}
				}
			}

			return false;
		};

		var isError = false;
		for (var mark_ of marks)
		{
			let mark = getRule(mark_);

			mark.errorInCondition = isErrorCondition(mark);
			mark.errorInConditionRecursive = isErrorInConditionRecursive(rules, mark.crules);

			isError = isError || mark.errorInCondition || mark.errorInConditionRecursive;
		}

		return isError;
	};

	// Вызов этой функции нужен для корректного заполнения массива topLevelRules
	// Однако функция уже должна быть вызвана ранее, но всё равно на всякий случай она вызывается
	this.setTopLevelRules();
	isErrorInConditionRecursive(settings.rules, this.topLevelRules);

	var urlsToText = function(urls)
	{
		if (!urls || urls.length <= 0)
			return '*';
		console.error(urls);
		return urls.join('   |   ');
	};

	var create = HTTPUACleaner.html;

	var main = new create('div');
	main.style.width = '100%';
	main.id  = 'rules-MainTable';

	var setGlobalRules = function(rules, crules, main, SideLogURL, tabPadding)
	{
		// Это означает, что нужно выводить только правила верхнего уровня - без предков
		if (crules === null)
		{
			crules = rules;
		}
		else
		if (crules.length <= 0)
			return 0;

		var t  = new create('table', main);
		t.style.width = '100%';

		if (crules !== rules && tabPadding != 0)
			t.style['padding-left'] = '2em';

		// mark - это правило
		var setGRuleFunc = function(mark, td)
		{
			var t    = new create('table', td);
			
			if (mark.rulemoving)
				t.style.border = 'solid red 3px';

			
			var tr1  = new create('tr',    t);
			var tr2  = new create('tr',    t);
			var tr3  = new create('tr',    t);

			var de   = new create('td',    tr1);
			var dprv = new create('td',    tr1);
			var d1   = new create('td',    tr1);
			var dn   = new create('td',    tr1);
			var dadd = new create('td',    tr1);
			var dcp  = new create('td',    tr1);
			var drm  = new create('td',    tr1);

			var dt   = new create('td',    tr2);
			var dd   = new create('td',    tr2);
			var dp   = new create('td',    tr2);
			var d2   = new create('td',    tr2);
			var dlt  = new create('td',    tr2);

			dlt.colSpan = 3;
			d2 .colSpan = tr1.html.length - tr2.html.length + 1 - dlt.colSpan + 1;

			var td3a = new create('td', tr3);
			var td3  = new create('td', tr3);
			var tde  = new create('td', tr3);
			td3a.colSpan = 3;
			tde .colSpan = 3;
			td3 .colSpan = tr1.html.length - td3a.colSpan - tde.colSpan;
			

			td3a.style['text-align'] = 'center';
			td3a.textContent = '+';

			t.border = '1px';
			t .style['border-collapse'] = 'collapse';
			t.style.width = '100%';

			d1  .text (mark.priority);
			d1  .color('white', '#000000');
			dprv.text(mark.showChildsStr[mark.showChilds]);
			dprv.color('white', '#000000');
			dn.text (mark.name);
			
			var deTC = mark.getEnabledTextAndColor();

			de.text (deTC[0]);
			de.color(deTC[1], deTC[2]);

			dadd .text ('+');
			dcp  .text ('>');
			drm  .text ('X');

			dadd .style['text-align'] = 'center';
			dcp  .style['text-align'] = 'center';
			drm  .style['text-align'] = 'center';
			dprv .style['text-align'] = 'center';
			dp   .style['text-align'] = 'center';
			d1   .style['text-align'] = 'center';
			de   .style['text-align'] = 'center';
			dt   .style['text-align'] = 'center';
			dd   .style['text-align'] = 'center';
			
			dadd .style['width'] = '1em';
			dcp  .style['width'] = '1em';
			drm  .style['width'] = '1em';

			dlt.text(mark.lastTime == 0 ? '-' : new Date(mark.lastTime).toLocaleDateString());
			dlt.style['text-align'] = 'center';
			tde.style['text-align'] = 'center';


			var txt2 = mark.tc[mark.isTabCondition];
			dt.text (txt2);
			dd.text (mark.pc[mark.isPath]);
			dp.text (mark.priorityToShow);
			d2.text (mark.conditionStr);
			
			if (mark.errorInCondition)
			{
				dt.color('white', 'red');
				d2.color('white', 'red');
			}
/*
			if (mark.errorInConditionRecursive)
			{
				dn.color('black', 'red');
			}
*/
			var setData = function(td, type)
			{
				td.data = {};
				td.data.number = mark.number;
				td.data.type   = 'rule';
				td.data.etype  = type;
				td.id          = td.data.type + td.data.etype + mark.number;
				td.data.mousedown = true;
			};

			de  .style['width'] = '2em';
			dprv.style['width'] = '2em';
			d1  .style['width'] = '2em';
			dt  .style['width'] = '2em';
			dd  .style['width'] = '2em';
			dp  .style['width'] = '2em';
			
			setData(de, 'enabled');
			//setData(dprv, 'PRV');
			setData(dprv, 'showChilds');
			setData(d1, 'priority');
			setData(dp, 'priorityToShow');
			setData(dn, 'name');
			setData(dt, 'isTabCondition');
			setData(dd, 'isPath');
			setData(d2, 'condition');
			setData(td3a,'filter-add');
			setData(td3, 'filters');
			setData(dlt, 'rulemovestart');
			setData(tde, 'rulemoveend');

			// Иначе logBView.js превентит редактирование условия фильтра
			d2.data['mousedown-noPreventDefault'] = true;
			dn.data['mousedown-noPreventDefault'] = true;
			
			// Иначе logBView.js превентит выпадающий список
			td3a.data['mousedown-noPreventDefault'] = true;
			
			setData(dadd , 'add');
			//setData(daddu, 'addu');
			setData(dcp,  'cp');
			//setData(dcpu, 'cpu');
			setData(drm,  'rm');
			//setData(drmu, 'rmu');

			this.getHtmlTab_Filters(settings, mark, td3);
			
			return tde;
			//text (d3, urlsToText(mark.reqRules));
		}.bind(this);

		var getRule = function(rule)
		{
			if (rule instanceof HTTPUACleaner.sdbP.Rule)
				return rule;

			return rules[rule];
		};

		var filtered = 0;
		var rulesToShow = [];

		for (var markName in crules)
		{
			if (crules === rules && !(crules[markName] instanceof HTTPUACleaner.sdbP.Rule))
				continue;

			var mark = getRule(crules[markName]);

			if (!(mark instanceof HTTPUACleaner.sdbP.Rule))
				continue;
			/*
			// Берём только правила верхнего уровня
			if (crules === rules && mark.prule != null)
				continue;
*/
			// Убираем правила, явно не соответствующие домену, но только если они верные
			if (domain && !mark.errorInConditionRecursive && !mark.errorInCondition
					&& !mark.preCheckCondition(domain, lastUrls.CurrentHost))
			{
				filtered++;
				continue;
			}
			
			rulesToShow.push(mark);
		}

		rulesToShow.sort
		(
			function(r1, r2)
			{				
				if (r1.conditionStr == '' && r2.conditionStr != '')
					if (r1.crules.length <= 0 && r1.filters.length <= 0)
						return 1;

				if (r1.conditionStr != '' && r2.conditionStr == '')
					if (r2.crules.length <= 0 && r2.filters.length <= 0)
						return -1;

				if (r1.priorityToShow != r2.priorityToShow)
					return r1.priorityToShow - r2.priorityToShow;

				if (r1.priority - r2.priority != 0)
					return r1.priority - r2.priority;

				return r1.name.localeCompare(r2.name);
			}
		);

		for (var markI in rulesToShow)
		{
			var mark = rulesToShow[markI];

			var tr = new create('tr', t);
			var td = new create('td', tr);
			td.color(false, '#FFFFFF');
			//td.style['padding-left']  = '2em';
			//td.style['padding-right'] = '2em';
			td.style['min-width'] = '30%';
			td.data        = {};
			td.data.number = mark.number;

			td.style.padding = '0px 0px 0px 0px';
			td.textEnabled(mark.enabled, true);
			tr.id = 'rrow'   + mark.number;
			td.id = 'rtable' + mark.number;

			var tde  = setGRuleFunc(mark, td);
			var tdeF = '';

			if (mark.showChilds > 0)
			    tdeF = setGlobalRules(rules, mark.crules, t, SideLogURL, tabPadding + 1);
			else
				tdeF = mark.crules.length;

			tde.text(tdeF ? tdeF : '');
			if (mark.errorInConditionRecursive)
			{
				tde.color('white', 'red');
			}
		}
		
		return filtered;
	}.bind(this);

	if (!settings)
		settings = this.currentSettings;

	setGlobalRules(settings.rules, this.topLevelRules, main, SideLogURL, 0);

	main.scrool = scrool;

	return main;
};

HTTPUACleaner.html = function(tagName, parent)
{
	this.tag = tagName;
	this.style = {font: HTTPUACleaner.html.font};

	if (parent)
		parent.add(this);

	return this;
};

// setPreferences
// HTTPUACleaner.html.font = p.get(HTTPUACleaner_Prefix + 'mainpanel.font', undefined);

HTTPUACleaner.html.prototype.urls = require('./getURL');

HTTPUACleaner.html.prototype.add = function(child)
{
	if (!this.html)
		this.html = [];

	this.html.push(child);
	
	return this;
};

HTTPUACleaner.html.prototype.font = function(fontName)
{
	this.style['font-family'] = fontName;
	
	return this;
};

HTTPUACleaner.html.prototype.text = function(text, truncCount, localization)
{
	if (text === true)
	{
		text = this.str.YES; //'YES';
	}
	else
	if (text === false)
	{
		text = this.str.NO; //'NO';
	}

	if (!text || !text.length)
		text = '' + text;

	this.textContent = this.urls.truncateString(localization ? _(text) : text, truncCount);
	
	return this;
};


HTTPUACleaner.html.prototype.textEnabled = function(text, noText)
{
	if (text)
	{
		if (!noText)
		this.textContent = '+';
		this.color(false, '#FFFFFF');
	}
	else
	{
		if (!noText)
		this.textContent = '-';
		this.color(false, '#AAAAAA');
	}
	
	return this;
};

HTTPUACleaner.html.prototype.color = function(color, bgcolor)
{
	if (color)
		this.style['color'] = color;
	if (bgcolor)
		this.style['background-color'] = bgcolor;
	
	return this;
};

HTTPUACleaner.html.prototype.time = function(time)
{
	var to2 = function(str)
	{
		str = str.toString();
		
		if (str.length > 1)
			return str;
		
		return '0' + str;
	};
	
	var t = new Date(time);
	this.textContent = to2(t.getHours()) + ':' + to2(t.getMinutes()) + ':' + to2(t.getSeconds());
	
	return this;
};

HTTPUACleaner.html.prototype.str = {'NO': HTTPUACleaner['sdk/l10n'].get('NO'), 'YES': HTTPUACleaner['sdk/l10n'].get('YES')};
