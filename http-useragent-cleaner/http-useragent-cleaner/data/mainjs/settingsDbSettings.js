/*

Набор правил хранится в файле с именем по типу 
C:\Users\UserName\AppData\Roaming\Mozilla\Firefox\Profiles\8njk7wtvzx.default-1485604532545\HTTPUACleaner\HttpUserAgentCleaner.opt

Сами правила в памяти состоят из двух частей - settings и currentSettings
currentSettings содержит текущие правила, включая временные
settings - объект для загрузки, который удаляется после загрузки функцией getOptions

Правила содержат следующие объекты
rules - набор правил типа HTTPUACleaner.sdbP.Rule, включая глобальную переменную maxNumber для уникальной нумерации правил

*/
HTTPUACleaner.sdbP.prototype.urls = require('./getURL');

HTTPUACleaner.sdbP.prototype.getOptions = function()
{
	if (!this.settings)
		this.settings = {};
	
	if (!this.settings.rules)
	{
		this.settings.rules = {maxNumber: 0};
		new HTTPUACleaner.sdbP.Rule(this.settings, false);
	}
	
	if (this.getRulesCount(this.settings) < 1)
	{
		this.settings.rules.maxNumber = 0;
		new HTTPUACleaner.sdbP.Rule(this.settings, false);
	}

	this.currentSettings = {rules: {}};
	if (this.settings.lastSave)
		this.currentSettings.lastSave = this.settings.lastSave;

	this.currentSettings.rules.maxNumber = this.settings.rules.maxNumber;
	if (this.settings.http)
		this.currentSettings.http = this.settings.http;

	this.topLevelRules = [];
	
	// Преобразуем правила из объектов JSON в объекты правил
	for (var ruleName in this.settings.rules)
	{
		if (ruleName == 'maxNumber')
			continue;
		
		var rule = this.settings.rules[ruleName];
		if (rule instanceof HTTPUACleaner.sdbP.Rule)
		{
			this.currentSettings.rules[rule.number] = rule;
			continue;
		}

		var newRule = new HTTPUACleaner.sdbP.Rule(this.currentSettings, false, rule);
		this.currentSettings.rules[newRule.number] = newRule;
	}

	for (var ruleName in this.currentSettings.rules)
	{
		if (ruleName == 'maxNumber')
			continue;

		var newRule = this.currentSettings.rules[ruleName];
		for (var i in newRule.filters)
		{
			// instanceof не работает, т.к. число - не объект
			if (newRule.filters[i] === Number(newRule.filters[i]))
			{
				newRule.filters[i] = [{val: newRule.filters[i], regime: 0}];
			}
			else
			if (newRule.filters[i] instanceof Array)
			{
				// nothing
			}
			else
			{
				newRule.filters[i] = [newRule.filters[i]];
			}
		}

		for (var i in newRule.filters)
		for (var j in newRule.filters[i])
		{
			var rf = newRule.filters[i][j];
			if (rf.log === undefined)
			{
				if (i == 'Request')
					rf.log = rf.val ;
				else
					rf.log = 0;
			}
		}
	}

	delete this.settings;

	this.setTopLevelRules();
	
	this.initHttpOptions();

	if (HTTPUACleaner.allBlock !== false && !this.setAllBlock)
	{
		HTTPUACleaner.allBlock++;
		this.setAllBlock = true;
		HTTPUACleaner.setPluginButtonState();
	}

	return this.currentSettings;
};

HTTPUACleaner.sdbP.prototype.getRulesCount = function(settings)
{
	// -1 - это потому, что кроме самих правил, есть ещё число maxNumber
	return Object.keys(settings.rules).length - 1;
};

HTTPUACleaner.sdbP.prototype.deleteRule = function(settings, ruleNumber, noSave, noRemove)
{
	var rule = settings.rules[ruleNumber];

	if (this.response && !noRemove)
	{
		this.response
		(
			{
				query:  'remove',
				id:     'rrow' + rule.number
			}
		);
	}

	if (rule.prule !== null)
		settings.rules[rule.prule].deleteChild(rule);
	
	for (var cruleNumber of rule.crules)
	{
		var cr = settings.rules[cruleNumber];
		if (!cr)
		{
			console.error('HUAC soft error: while rule deleting lost child rule has been find ' + cruleNumber + ' in ' + ruleNumber);
			continue;
		}

		cr.prule = null;
		this.deleteRule(settings, cruleNumber, true, noRemove || !rule.showChilds);
	}

	delete settings.rules[ruleNumber];


	if (this.getRulesCount(this.currentSettings) < 1)
		this.getOptions();

	if (!noSave)
		this.setOptions();
};

HTTPUACleaner.sdbP.prototype.isChild = function(ruleNumber, parentNumber)
{
	var settings = this.currentSettings;
	var rule   = settings.rules[ruleNumber];
	var parent = settings.rules[parentNumber];
	
	if (!rule || !parent)
	{
		console.error('HUAC ERROR: HTTPUACleaner.sdbP.prototype.isChild: !rule || !parent ' + ruleNumber + '/' + parentNumber);
		throw new Error('!rule || !parent');
	}

	if (parent.crules.indexOf(ruleNumber) >= 0)
		return true;

	for (var p of parent.crules)
	{
		if (this.isChild(ruleNumber, p) === true)
			return true;
	}

	return false;
};

HTTPUACleaner.sdbP.prototype.moveRule = function(ruleNumber, newParentRuleNumber)
{
	var settings = this.currentSettings;
	var rule   = settings.rules[ruleNumber];
	var parent = settings.rules[newParentRuleNumber];

	if (!rule || !parent)
	{
		console.error('HUAC ERROR: HTTPUACleaner.sdbP.prototype.isChild: !rule || !parent ' + ruleNumber + '/' + newParentRuleNumber);
		console.error(rule);
		console.error(parent);
		throw new Error('!rule || !parent');
	}

	delete rule.rulemoving;

	// Если правило перемещается само в себя, это значит, что оно должно стать правилом верхнего уровня
	if (ruleNumber === newParentRuleNumber)
	{
		if (rule.prule === null)
			return;

		parent = settings.rules[rule.prule];
		var a = parent.crules.indexOf(ruleNumber);
		if (a < 0)
		{
			console.error('HUAC ERROR: HTTPUACleaner.sdbP.prototype.moveRule: parent.crules.indexOf(ruleNumber) < 0');
			return;
		}

		parent.crules.splice(a, 1);
		rule.prule = null;
	}
	else
	{
		// Если правило уже подчинено новому родителю
		if (rule.prule == newParentRuleNumber)
			return;
		
		var deleteParent2 = function()
		{
			// Если перемещаемое правило подчинено другому правилу
			if (rule.prule !== null)
			{
				// parent2 - родительское правило перемещаемого правила
				var parent2 = settings.rules[rule.prule];
				var a = parent2.crules.indexOf(ruleNumber);
				if (a < 0)
				{
					console.error('HUAC ERROR: HTTPUACleaner.sdbP.prototype.moveRule: parent2.crules.indexOf(ruleNumber) < 0');
					return false;
				}

				parent2.crules.splice(a, 1);
				return parent2;
			}

			return false;
		};

		// Если новое правило-родитель сейчас непосредственно подчинено перемещаемому правилу непосредственно
		if (parent.prule == ruleNumber)
		{
			// В перемещаемом правиле rule ищем подчинённое правило newParentRuleNumber, которое будет родительским
			var a = rule.crules.indexOf(newParentRuleNumber);
			if (a < 0)
			{
				console.error('HUAC ERROR: HTTPUACleaner.sdbP.prototype.moveRule: parent.crules.indexOf(newParentRuleNumber) < 0');
				return;
			}
			rule.crules.splice(a, 1);

			var parent2 = deleteParent2();
			if (parent2)
				parent2.crules.push(newParentRuleNumber);

			parent.prule = rule.prule;
			rule.prule   = newParentRuleNumber;

			parent.crules.push(ruleNumber);
		}
		else
		{
			if (this.isChild(newParentRuleNumber, rule.number))
			{
				console.error('HUAC Exclamation: HTTPUACleaner.sdbP.prototype.moveRule: the moving rule is the parent of the new parent rule: moving cancelled (use the new blank rule for help)');
				return;
			}

			deleteParent2();
			rule.prule   = newParentRuleNumber;

			parent.crules.push(ruleNumber);
		}
	}
	
	this.setOptions();
};

HTTPUACleaner.sdbP.prototype.setTopLevelRules = function()
{
	this.topLevelRules = [];
	for (var ruleName in this.currentSettings.rules)
	{
		var rule = this.currentSettings.rules[ruleName];
		if (rule instanceof HTTPUACleaner.sdbP.Rule)
		{
			if (rule.prule == null)
				this.topLevelRules.push(rule);
		}
	}
};

// Это функция сохранения
HTTPUACleaner.sdbP.prototype.setOptions = function(callback)
{
	var f = function(result, t)
	{
		delete t.settings;
		if (callback)
			callback();
	};

	this.setTopLevelRules();

	var sc        = this.currentSettings;
	this.settings = JSON.parse(JSON.stringify(sc));
	var settings  = this.getOptions();
	this.currentSettings = sc;
	
	// Этот вызов нужен в связи с тем, что getOptions тоже вызывает эту функцию. И массив становится невалидным.
	this.setTopLevelRules();

	var rules = settings.rules;
	for (var ruleName in rules)
	{
		var rule = rules[ruleName];
		if (rule.isTemporary)
		{
			this.deleteRule(settings, ruleName, true, true);
		}
		else
			delete rule.rulemoving;
	}

	this.saveSettings(settings, f, sc, !callback);
};


HTTPUACleaner.sdbP.prototype.event = function(data)
{
	if (data.query == 'mousedown')
	{
		this.event.mousedown.bind(this)(data);
	}
};


/*
data = 
{
	query: 'mousedown',
	crtl:  false,
	shift: false,
	id:    'rulepriority2',
	data:
	{
		number: 2,
		type: 'rule',
		etype: 'priority',
		mousedown: true
	}
}
*/
//  Вызывать this.event.mousedown.bind(this)
HTTPUACleaner.sdbP.prototype.event.mousedown = function(data)
{
	if (!this.currentSettings)
	{
		console.error('HUAC error: HTTPUACleaner.sdbP.currentSettings has not initialized');
		return;
	}

	if (!this.response)
	{
		console.error('HUAC error: response in HTTPUACleaner.sdbP has not been setted');
		console.error(data);
		return;
	}

	if (data.data.type == 'rule')
	{
		HTTPUACleaner.sdbP.prototype.event.mousedown.rule.bind(this)(data);
		
		this.setOptions();
		
		return;
	}
	
};

//  Вызывать this.event.mousedown.rule.bind(this)
HTTPUACleaner.sdbP.prototype.event.mousedown.rule = function(data)
{
	var rule = this.currentSettings.rules[data.data.number];
	if (!rule)
	{
		console.error('HUAC error: rule in HTTPUACleaner.sdbP.event.mousedown has not been setted ' + data.number);
		console.error(data);
		return;
	}

	// Перемещение правил
	if (data.data.etype == 'rulemovestart')
	{
		if (this.rulemove)
		{
			delete this.currentSettings.rules[this.rulemove.moverule].rulemoving;

			if (this.rulemove.moverule == rule.number)
			{
				this.rulemove = false;
				
				// Перерисовываем всю страницу и пытаемся оказаться, при этом, на том же самом месте
				HTTPUACleaner.getSideLog({SideLogURL: this.lastSideLogURL, scrool: data.scrool, CurrentHost: this.lastUrls.CurrentHost, CurrentDomain: this.lastUrls.CurrentDomain});

				return;
			}
		}

		this.rulemove = {};
		this.rulemove.moverule = rule.number;
		rule.rulemoving = true;

		// Перерисовываем всю страницу и пытаемся оказаться, при этом, на том же самом месте
		HTTPUACleaner.getSideLog({SideLogURL: this.lastSideLogURL, scrool: data.scrool, CurrentHost: this.lastUrls.CurrentHost, CurrentDomain: this.lastUrls.CurrentDomain});
		
		return;
	}

	if (data.data.etype == 'rulemoveend')
	{
		if (!this.rulemove)
			return;

		this.moveRule(this.rulemove.moverule, data.data.number);
		this.rulemove = false;

		// Перерисовываем всю страницу и пытаемся оказаться, при этом, на том же самом месте
		HTTPUACleaner.getSideLog({SideLogURL: this.lastSideLogURL, scrool: data.scrool, CurrentHost: this.lastUrls.CurrentHost, CurrentDomain: this.lastUrls.CurrentDomain});

		return;
	}

	// Изменяем приоритет правила
	if (data.data.etype == 'priority')
	{
		if (!data.shift)
			rule.priority++;
		else
			rule.priority--;

		if (rule.priority > 20)
			rule.priority = 0;
		if (rule.priority < 0)
			rule.priority = 20;

		this.response
		(
			{
				query: 'mousedown',
				id:    data.id,
				name: 'textContent',
				text: '' + rule.priority
			}
		);
		
		return;
	}

	// Изменяем приоритет правила для отображения
	if (data.data.etype == 'priorityToShow')
	{
		if (!data.shift)
			rule.priorityToShow++;
		else
			rule.priorityToShow--;

		if (rule.priorityToShow > 9)
			rule.priorityToShow = 0;
		if (rule.priorityToShow < 0)
			rule.priorityToShow = 9;

		this.response
		(
			{
				query: 'mousedown',
				id:    data.id,
				name: 'textContent',
				text: '' + rule.priorityToShow
			}
		);
		
		return;
	}


	// Показываем или не показываем потомков
	if (data.data.etype == 'showChilds')
	{
		if (!data.shift)
		{
			rule.showChilds++;
			if (rule.showChilds >= 2)
				rule.showChilds = 0
		}
		else
		{
			rule.PRV--;
			if (rule.showChilds < 0)
				rule.showChilds = 2;
		}

		this.response
		(
			{
				query: 'mousedown',
				id:    data.id,
				name: 'textContent',
				text: rule.showChildsStr[rule.showChilds]
			}
		);
		
		// Перерисовываем всю страницу и пытаемся оказаться, при этом, на том же самом месте
		HTTPUACleaner.getSideLog({SideLogURL: this.lastSideLogURL, scrool: data.scrool, CurrentHost: this.lastUrls.CurrentHost, CurrentDomain: this.lastUrls.CurrentDomain});
		
		return;
	}

	/*
	// Не стал делать правила для работы в приватном режиме, т.к. в content-policy приватный режим ещё хрен знает как сделать
	// Изменяем режим PRV правила (работа в приватном режиме)
	if (data.data.etype == 'PRV')
	{
		if (!data.shift)
		{
			rule.PRV++;
			if (rule.PRV > 2)
				rule.PRV = 0
		}
		else
		{
			rule.PRV--;
			if (rule.PRV < 0)
				rule.PRV = 2;
		}

		this.response
		(
			{
				query: 'mousedown',
				id:    data.id,
				name: 'textContent',
				text: rule.prvTo[rule.PRV]
			}
		);
		
		return;
	}*/
	
	// Включаем или отключаем правило
	if (data.data.etype == 'enabled')
	{
		if (data.shift)
		{
			rule.isTemporary = !rule.isTemporary;
		}
		else
		rule.enabled = !rule.enabled;

		var deTC = rule.getEnabledTextAndColor();
	
		// Запрос на отображение изменения состояния правила
		this.response
		(
			{
				query: 'mousedown',
				id:    data.id,
				name: 'textContent',
				text: deTC[0],
				style: {'background-color': deTC[2], color: deTC[1]}
			}
		);
		
		// Запрос на перекрашивание самого правила (его фона)
		this.response
		(
			{
				query: 'mousedown',
				id:    'rtable' + rule.number,
				style: {'background-color': rule.enabled ? '#FFFFFF' : '#AAAAAA'}
			}
		);

		// Безусловно, даже если не было, отменяем удаление правила, если правило включили
		if (rule.enabled)
		{
			var dt = function() {};
			dt.prototype = data;
			dt     = new dt();
			dt.data.etype = 'rm-unconfirm';
			dt.id   = 'rulerm' + rule.number;
			this.event.mousedown.rule.bind(this)(dt);
		}

		// this.checkTemporaryCondition();
		
		return;
	}
	
	// Изменяем переключатель tab/req
	if (data.data.etype == 'isTabCondition')
	{
		if (!data.shift)
		{
			rule.isTabCondition++;
			if (rule.isTabCondition >= Object.keys(rule.tc).length)
				rule.isTabCondition = 0;
		}
		else
		{
			rule.isTabCondition--;
			if (rule.isTabCondition < 0)
				rule.isTabCondition = Object.keys(rule.tc).length - 1;
		}

		this.response
		(
			{
				query: 'mousedown',
				id:    data.id,
				name: 'textContent',
				text: rule.tc[rule.isTabCondition]
			}
		);
		
		return;
	}
	
	// Изменяем переключатель domain/path
	if (data.data.etype == 'isPath')
	{
		if (!data.shift)
		{
			rule.isPath++;
			if (rule.isPath >= Object.keys(rule.pc).length)
				rule.isPath = 0;
		}
		else
		{
			rule.isPath--;
			if (rule.isPath < 0)
				rule.isPath = Object.keys(rule.pc).length - 1;
		}

		this.response
		(
			{
				query: 'mousedown',
				id:    data.id,
				name: 'textContent',
				text: rule.pc[rule.isPath]
			}
		);
		
		return;
	}
	
	// При клике на заголовок правила, его имя должно начать редактироваться
	if (data.data.etype == 'name')
	{
		this.response
		(
			{
				query:  'mousedown-change-name',
				id:     data.id,
				number: data.data.number,
				text:   rule.name,
				host:    this.lastUrls.CurrentHost,
				domain:  this.lastUrls.CurrentDomain
			}
		);
		
		return;
	}
	
	// Закончено изменение имени правила
	if (data.data.etype == 'name-changed')
	{
		rule.name = new String(data.text);

		// Удаляет поле ввода нового имени
		this.response
		(
			{
				query:  'remove',
				id:     data.id2
			}
		);

		this.response
		(
			{
				query:  'mousedown',
				id:     data.id,
				name:   'textContent',
				text:   rule.name
			}
		);

		return;
	}

	// Клик на кнопку X в заголовке правила для начала процедуры удаления
	if (data.data.etype == 'rm')
	{
		// Если это повторный клик в нужный момент времени
		if (data.data['rm-confirm'] === true && !rule.enabled)
		{
			this.deleteRule(this.currentSettings, rule.number, true);
		}
		else
		{
			// Если это первый клик - мы отключаем правило
			if (rule.enabled)
			{
				var dt = function() {};
				dt.prototype = data;
				dt     = new dt();
				dt.data.etype = 'enabled';
				dt.id  = 'ruleenabled' + rule.number;
				this.event.mousedown.rule.bind(this)(dt);
			}

			// Даём сообщение интерфейсу, чтобы он начал свои блуждания по состояниям и установил data.data['rm-confirm']
			this.response
			(
				{
					query: 'rm-confirm',
					id:    data.id
				}
			);
		}

		return;
	}
	
	// Если пользователь включил правило, то, в этой же процедуре, будет и отмена состояния удаления правила
	if (data.data.etype == 'rm-unconfirm')
	{
		this.response
		(
			{
				query: 'rm-unconfirm',
				id:    data.id,
				name: 'textContent',
				text: 'X'
			}
		);

		return;
	}
	
	
	// Добавляем новое правило
	if (data.data.etype == 'add')
	{
		var newRule  = new HTTPUACleaner.sdbP.Rule(this.currentSettings, data.shift);
		newRule.enabled = false;

		if (rule.prule)
		{
			var pr = this.currentSettings.rules[rule.prule];
			pr.addChild(newRule);
			newRule.name = pr.name + '.' + pr.crules.length;
		}
		else
			newRule.name = '' + this.getRulesCount(this.currentSettings);

		// Перерисовываем всю страницу и пытаемся оказаться, при этом, на том же самом месте
		HTTPUACleaner.getSideLog({SideLogURL: this.lastSideLogURL, scrool: data.scrool, CurrentHost: this.lastUrls.CurrentHost, CurrentDomain: this.lastUrls.CurrentDomain});

		return;
	}
	
	// Добавляем новое правило как подчинённое другому правилу
	if (data.data.etype == 'cp')
	{
		var newRule  = new HTTPUACleaner.sdbP.Rule(this.currentSettings, data.shift);
		newRule.enabled = false;

		rule.addChild(newRule);
		newRule.name = rule.name + '.' + rule.crules.length;

		// Перерисовываем всю страницу и пытаемся оказаться, при этом, на том же самом месте
		HTTPUACleaner.getSideLog({SideLogURL: this.lastSideLogURL, scrool: data.scrool, CurrentHost: this.lastUrls.CurrentHost, CurrentDomain: this.lastUrls.CurrentDomain});

		return;
	}
	
	if (data.data.etype == 'filter-add')
	{
		var filters = [];
		for (var fi in rule.filtersNames)
		{
			var fn = rule.filtersNames[fi];
			// Исключаем те фильтры, которые уже добавлены
			// Теперь не стоит это делать, т.к. можно добавить сразу два фильтра: круглый и треугольник (с разными режимами)
			if (!rule.filters[fn] || rule.filters[fn].length < 2)
				filters.push([fn, rule.filtersLocaled[fn]]);
		}

		filters.push(['', '']);


		var html = new HTTPUACleaner.html('select');
		html.id  = 'rulefilter-add-select' + rule.number;
		for (var fi in filters)
		{
			var opt  = new HTTPUACleaner.html('option');
			html.add(opt);
			opt.text(filters[fi][1]);
			opt.value = filters[fi][0];
			/*
			if (filters[fi][0] == '')
				opt.disabled = true;*/
		}

		this.response
		(
			{
				query:   'filter',
				squery:  ['add', 'show'],
				id:      'rulefilter-add' + rule.number,
				filters: filters,
				html:    html
			}
		);

		return;
	}
	
	if (data.data.etype == 'filter')
	{
		if (data.data.stype[0] == 'change' && data.data.stype[1] == 'changed')
		{
			var html = null;
			if (data.text != '')
			{
				var nr = 0;

				if (!rule.filters[data.text])
				{
					if (data.text == 'Request')
						rule.filters[data.text] = [{val: 0, regime: 0, log: 1}];
					else
						rule.filters[data.text] = [{val: 0, regime: 0}];
				}
				else
				if (rule.filters[data.text].length < 2)
				{	
					var log = 0;
					if (data.text == 'Request')
						log = 1;

					if (rule.filters[data.text][0].regime == 0)
						rule.filters[data.text].push({val: 0, regime: 1, log: log});
					else
						rule.filters[data.text].push({val: 0, regime: 0, log: log});
				}
				else
					nr = -1;

				if (nr >= 0)
				{
					html = this.getHtmlTab_Filters(this.currentSettings, rule, undefined);
				}
			}

			this.response
			(
				{
					query:   'filter',
					squery:  ['add', 'end'],
					id:      data.id, //'rulefilter-add' + rule.number,
					id2:     data.id2, //'rulefilter-add-select' + rule.number
					id3:	 'rulefilters' + rule.number,
					html:    html
				}
			);
		}
	}
	
	if (data.data.etype == 'filter-filter')
	{
		var fName  = data.data.fName;
		var fIndex = data.data.fIndex;
		var fs     = rule.filtersStates[fName];
		var al     = fs.length;
		if (data.shift)
		{
			rule.filters[fName][fIndex].val--;
			if (rule.filters[fName][fIndex].val < 0)
				rule.filters[fName][fIndex].val = al - 1;
		}
		else
		{
			rule.filters[fName][fIndex].val++;
			
			if (rule.filters[fName][fIndex].val >= al)
				rule.filters[fName][fIndex].val = 0;
		}

		var html = this.getHtmlTab_Filters(this.currentSettings, rule, undefined);

		this.response
		(
			{
				query:   'filter',
				squery:  ['change', 'end'],
				id:	 	'rulefilters' + rule.number,
				html:    html
			}
		);
	}

	if (data.data.etype == 'filter-filterregime')
	{
		var fName = data.data.fName;
		var fIndex = data.data.fIndex;
		var fs    = rule.filtersStates[fName];
		var al    = 2;
		if (data.shift)
		{
			rule.filters[fName][fIndex].regime--;
			if (rule.filters[fName][fIndex].regime < 0)
				rule.filters[fName][fIndex].regime = al - 1;
		}
		else
		{
			rule.filters[fName][fIndex].regime++;
			
			if (rule.filters[fName][fIndex].regime >= al)
				rule.filters[fName][fIndex].regime = 0;
		}

		var html = this.getHtmlTab_Filters(this.currentSettings, rule, undefined);

		this.response
		(
			{
				query:   'filter',
				squery:  ['change', 'end'],
				id:	 	'rulefilters' + rule.number,
				html:    html
			}
		);
	}
	
	if (data.data.etype == 'filter-filterlog')
	{
		var fName = data.data.fName;
		var fIndex = data.data.fIndex;
		var fs    = rule.filtersStates[fName];
		var al    = 5;
		if (rule.filters[fName][fIndex].log === undefined)
			rule.filters[fName][fIndex].log = 0;

		if (data.shift)
		{
			rule.filters[fName][fIndex].log--;
			if (rule.filters[fName][fIndex].log < 0)
				rule.filters[fName][fIndex].log = al - 1;
		}
		else
		{
			rule.filters[fName][fIndex].log++;
			
			if (rule.filters[fName][fIndex].log >= al)
				rule.filters[fName][fIndex].log = 0;
		}

		var html = this.getHtmlTab_Filters(this.currentSettings, rule, undefined);

		this.response
		(
			{
				query:   'filter',
				squery:  ['change', 'end'],
				id:	 	'rulefilters' + rule.number,
				html:    html
			}
		);
	}

	if (data.data.etype == 'condition')
	{
		var html = this.getHtmlTab_MainCondition(this.currentSettings, rule, this.lastSideLogURL, undefined);

		this.response
		(
			{
				query:   'filter',
				squery:  ['change-main', 'begin'],
				id:	 	 'rulecondition' + rule.number, //'rulefilters-maincond-' + rule.number,
				html:    html,
				host:    this.lastUrls.CurrentHost,
				domain:  this.lastUrls.CurrentDomain
			}
		);
	}

	if (data.data.etype == 'condition-change-main')
	{
		if (data.data.stype[1] == 'addSubRules')
		{
			var cd = this.lastUrls.CurrentDomain;
			var ch = this.lastUrls.CurrentHost;

			if (!ch)
				return;

			// Массив описателей вкладок. Каждый содержит массив BlockInfo, в каждом элементе содержится url подгружаемого элемента
			var tabInfo = HTTPUACleaner.loggerB.FindTab(ch, true);

			if (!tabInfo || tabInfo.length <= 0)
				return;

			rule.setCondition('td[:]=' + ch);

			var findRule = function(rule, newCondition)
			{
				if (!rule.crules)
					return false;

				for (var r of rule.crules)
				{
					r = this.currentSettings.rules[r];

					if (r.isTabCondition !== 0)
						return false;

					if (r.conditionStr == newCondition)
						return r;
				}

				return false;
			}.bind(this);


			var domains = {};
			for (var tab of tabInfo)
			for (var bi  of tab.BlockInfo)
			{
				var host = HTTPUACleaner.getHostByURI(bi.url);
				var domain = HTTPUACleaner.getDomainByHost(host);

				if (!domains[domain])
					domains[domain] = {};

				domains[domain][host] = true;
			}

			var notCnd = 'td[:1]!=rd[:1]';
			var finded = findRule(rule, notCnd);
			var newRuleNot = finded;
			if (finded)
				newRuleNot = finded;
			else
			{
				newRuleNot  = new HTTPUACleaner.sdbP.Rule(this.currentSettings, data.shift);
				newRuleNot.enabled = false;
				rule.addChild(newRuleNot);
				newRuleNot.name = rule.name + '.' + '!=';
				newRuleNot.setCondition(notCnd);
			}

			for (var domaini in domains)
			{
				var newCondition, rn;
				if (domaini == cd)
				{
					newCondition = 'td[:1]=rd[:1]';
					rn = rule;
				}
				else
				{
					if (!domaini)
						continue;

					newCondition = 'rd[:1]=' + domaini;
					rn = newRuleNot;
				}

				var finded = findRule(rn, newCondition);
				if (finded)
				{
					newRule = finded;
				}
				else
				{
					var newRule  = new HTTPUACleaner.sdbP.Rule(this.currentSettings, data.shift);
					newRule.enabled = false;
					rn.addChild(newRule);
					newRule.name = rule.name + '.' + (domaini == cd ? '=' : domaini);
					newRule.setCondition(newCondition);
				}

				var domain = domains[domaini];
				for (var hosti in domain)
				{
					// -1 - это разделяющая точка
					var hsub = hosti.substr(0, hosti.length - domaini.length - 1);
					if (hosti == ch)
						newCondition = 'rd[:]=td[:]';
					else
						newCondition = 'rd[2:]=' + (hsub.length > 0 ? hsub : '_@_');

					var finded = findRule(newRule, newCondition);
					if (finded)
					{
					}
					else
					{
						var newRule2  = new HTTPUACleaner.sdbP.Rule(this.currentSettings, data.shift);
						newRule2.enabled = false;
						newRule.addChild(newRule2);
						newRule2.name = newRule.name + '.=';

						newRule2.setCondition(newCondition);

						if (hosti == ch)
							newRule2.priorityToShow = 4;
					}
				}
			}

			// Перерисовываем всю страницу и пытаемся оказаться, при этом, на том же самом месте, как и при обычном добавлении правил
			HTTPUACleaner.getSideLog({SideLogURL: this.lastSideLogURL, scrool: data.scrool, CurrentHost: this.lastUrls.CurrentHost, CurrentDomain: this.lastUrls.CurrentDomain});
		}
		else
		{
			rule.setCondition(data.text);

			this.response
			(
				{
					query:   'filter',
					squery:  ['change-main', 'end'],
					id:	 	 'rulecondition' + rule.number, //'rulefilters-maincond-' + rule.number,
					id2:	 data.id2, //'rulefilters-maincond-' + rule.number,
					text:    rule.conditionStr
				}
			);
		}

		return;
	}
}
