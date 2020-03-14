const {Cc, Ci, Cu, Cr, Cm, components} = require("chrome");

exports.logger = function()
{
	this.tabs = [];
	this._enabled = true;
	
	this.getEnabled = function()
	{
		return this._enabled;
	};
	
	this.setEnabled = function(state)
	{
		// console.error('logB setEnabled ' + state);

		this._enabled = state;
		
		if (!state)
			this.CleanLog();
	};

	Object.defineProperty(this, 'enabled', {get: this.getEnabled, set: this.setEnabled, enumerable : false, configurable : true});
};

exports.logger.prototype =
{
	urls:    require('./getURL'),

	CleanLog: function()
	{
		var tabs = this.urls.tabs;
		var I    = this.urls.utils;

		var dTabs = [];

		for (var ti in this.tabs)
		{
			var t = this.tabs[ti];
			if (t.tabUri == '')
				continue;

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
			dTabs.push(t);
			this.tabs.splice(ti, 1);
		}
	
		if (dTabs.length <= 0)
			return;
	
		// Смотрим на все удалённые вкладки, возможно, там есть полезная информация
		// Из неудалённых вкладок информация будет выбираться при запросе лога
		// Если её выбирать сразу, то туда будут попадать, возможно, только начинающиеся грузится документы и лог пустой вкладки переполнится
		var eTab = this.FindTab('');
		if (eTab)
		dTabs.push(eTab);

		var eTabs = this.getEmptyTabs(true, dTabs);
		if (!eTabs || eTab == eTabs)
		{
			return;
		}

		if (eTab)
		{
			for (var ti in this.tabs)
			{
				var t = this.tabs[ti];
				if (t.tabUri == '')
				{
					this.tabs.splice(ti, 1);
					break;
				}
			}
		}

		this.tabs.push(eTabs);
	},
	
	CleanAll: function()
	{
		this.tabs = [];
	},
	
	FindTab: function(tabUrl, forHost)
	{
		if (!tabUrl || tabUrl == 'about:newtab' || tabUrl == 'about:blank' || tabUrl.indexOf('about:neterror') == 0)
			tabUrl = '';

		if (tabUrl)
		tabUrl = this.urls.getURIWithoutScheme(tabUrl.split('#')[0]).toString();

		if (forHost)
			tabUrl = this.urls.getHostByURI('http://' + tabUrl);

		var result = [];
		for (var ti in this.tabs)
		{
			var t = this.tabs[ti].tabUri;

			if (forHost)
				t = this.urls.getHostByURI('http://' + t);

			if (t == tabUrl)
			{
				if (forHost)
					result.push(this.tabs[ti]);
				else
					return this.tabs[ti];
			}
		}

		return forHost ? result : null;
	},
	
	getEmptyTabs: function(all, thisTabs)
	{
		var tabsF = this.urls.tabs;
		if (thisTabs === undefined)
			thisTabs = this.tabs;

		var tabs = [];
		for (var t of thisTabs)
		{
			var url = this.urls.getURIWithoutScheme(t.BlockInfo[0].url.split('#')[0]) + '';

			// Если это пустая вкладка
			// Служебные запросы типа обновления, а также запросы от дополнений типа AnonymoX идут без вкладки в этом логе
			if (t.tabUri == '')
			{
				if (all)
				{
					tabs.push(t);
					continue;
				}
			}

			// Если это вкладка, в которой первый запрос совпадает с url вкладки, то, возможно, это вкладка вообще была заблокирована
			// Если она была заблокирована, то она должна быть отображена именно в логе пустой вкладки
			if (t.tabUri == url)
			{
				var found = false;
				// Ищем url, подгруженный на вкладке, который не совпадает с url вкладки
				for (var bi of t.BlockInfo)
				{
					if (  url != this.urls.getURIWithoutScheme(bi.url.split('#')[0]) + ''  )
					{
						found = true;
						break;
					}
				}

				// Если такой url найден, значит это большая вкладка (запрос шёл не только на один url), значит не добавляем
				if (!found)
				{
					tabs.push(t);
					continue;
				}
			}

			// Если вкладки почему-то нет в открытых вкладках
			var found = false;
			for (var tabIndex in tabsF)
			{
				try
				{
					var tabF   = tabsF[tabIndex];
					var tmp    = this.urls.fromTabDocument(this.urls.sdkTabToChromeTab(tabF));
					var taburl = tmp ? tmp.split('#')[0] : '';

					if (taburl != '' && t.tabUri == this.urls.getURIWithoutScheme(taburl))
					{
						found = true;
						break;
					}
				}
				catch (e)
				{
					// console.error(e);
				}
			}

			if (!found)
				tabs.push(t);
		}

		if (tabs.length <= 0)
		{/*
			console.error('getEmptyTabs null');
			console.error(this.tabs);*/
			return null;
		}

		if (tabs.length == 1 && tabs[0].tabUri == '')
			return tabs[0];


		var tabFound = {tabUri: '', redirected: false, BlockInfo: []};

		for (var tab of tabs)
		{
			for (var i = 0; i < tab.BlockInfo.length; i++)
			{
				tabFound.BlockInfo.push(tab.BlockInfo[i]);
			}
		}
		tabFound.BlockInfo.sort
		(
			function(a, b)
			{
				return a.time - b.time;
			}
		);
		
		if (tabFound.BlockInfo.length > this.maxCount)
		{
			tabFound.truncated = true;
			tabFound.tcount    = tabFound.BlockInfo.length - this.maxCount;

			tabFound.BlockInfo.splice(0, tabFound.BlockInfo.length - this.maxCount);
		}
		/*
		console.error('getEmptyTabs');
		console.error(this.tabs);
		console.error(tabFound);*/

		return tabFound;
	},

	maxCount: 1024,
	
	addOToLog: function(object)
	{
		this.addToLog(object.taburl, object.redirectTo, object.url, object.BlockInfo);
	},
	
	addToLog: function(tabUri, redirectTo, url, BlockInfo)
	{
		if (!this.enabled)
			return;
		
		var tabURISpec = tabUri;

		if (tabURISpec)
		if (tabURISpec == 'about:blank' || tabURISpec == 'about:newtab' || tabURISpec.indexOf('about:neterror') >= 0)
		{
			console.error('HUAC: in loggerB.addToLog tabURISpec is ' + tabURISpec + '  ' + url);
			console.error(BlockInfo);
			return;
		}

		if (!tabURISpec)
			tabURISpec = '';

		// Прибавляем пустую строку, т.к. из-за этого идёт какое-то преобразование типов
		// Так строки отображаются нормально, а не как массив String, и при этом, кажется, что-то с чем-то лучше сравниваетс¤
		if (tabURISpec)
		tabURISpec = this.urls.getURIWithoutScheme(tabURISpec.split('#')[0]) + '';

		var urla = url;
		if (redirectTo)
		{
			redirectTo = this.urls.getURIWithoutScheme(redirectTo.split('#')[0]) + '';
			urla       = this.urls.getURIWithoutScheme(url       .split('#')[0]) + '';
		}

		var tabFound = this.FindTab(tabURISpec);

		if (!BlockInfo)
			BlockInfo = {};
		
		if (!tabFound && redirectTo && urla == tabURISpec)
		{
			tabFound = this.FindTab(redirectTo);
		}

		BlockInfo.url = url;
		if (!tabFound)
		{
			if (redirectTo && urla == tabURISpec)
			{
				tabURISpec = redirectTo;
			}

			BlockInfo.time = Date.now();
			tabFound = {tabUri: tabURISpec, redirected: !!redirectTo, BlockInfo: [BlockInfo], startTime: Date.now()};
			this.tabs.push(tabFound);
/*
			console.error('new tab');
			console.error(redirectTo);
			console.error(urla);
			console.error(tabURISpec);
			console.error(this.tabs);*/
		}
		else
		{
			BlockInfo.time = tabFound.BlockInfo[tabFound.BlockInfo.length-1].time;
			BlockInfo.repeatedCount = tabFound.BlockInfo[tabFound.BlockInfo.length-1].repeatedCount;

			var bi1 = JSON.stringify(BlockInfo);
			var bi2 = tabFound.BlockInfo.length > 0 ? JSON.stringify(tabFound.BlockInfo[tabFound.BlockInfo.length-1]) : '';

			BlockInfo.time = Date.now();
			BlockInfo.repeatedCount = 0;

			if (bi1 == bi2 && BlockInfo.time - tabFound.BlockInfo[tabFound.BlockInfo.length-1].time < 5*1000)
			{
				if (tabFound.BlockInfo[tabFound.BlockInfo.length-1].repeatedCount)
					tabFound.BlockInfo[tabFound.BlockInfo.length-1].repeatedCount++;
				else
					tabFound.BlockInfo[tabFound.BlockInfo.length-1].repeatedCount = 1;
			}
			else
			if (tabFound.BlockInfo.length > this.maxCount)
			{
				tabFound.BlockInfo.push(BlockInfo);
				if (tabFound.truncated === true)
				{
					tabFound.tcount++;
					tabFound.BlockInfo.shift();
				}
				else
				{
					tabFound.truncated = true;
					tabFound.tcount    = 1;
				}
			}
			else
				tabFound.BlockInfo.push(BlockInfo);

			if (redirectTo && urla == tabURISpec)
			{
				var ru = this.urls.getURIWithoutScheme(redirectTo.split('#')[0]) + '';
				var tabFoundRedirect = this.FindTab(ru);

				/*
				console.error('redirectTo');
				console.error(redirectTo);
				console.error(urla);
				console.error(tabURISpec);

				console.error(tabFound);
				console.error(tabFoundRedirect);
				console.error(tabFound !== tabFoundRedirect);
				console.error(this.tabs);
*/
				if (tabFoundRedirect && tabFound !== tabFoundRedirect)
				{
					for (var ti in this.tabs)
					{
						var t = this.tabs[ti];
						if (t.tabUri == tabURISpec)
						{
							this.tabs.splice(ti, 1);
							break;
						}
					}

					var ar = tabFoundRedirect.BlockInfo;
					var au = tabFound        .BlockInfo;
					
					var ir = ar.length - 1;
					var iu = au.length - 1;

					var cnt = 0;
					tabFoundRedirect.BlockInfo = [];
					while (cnt < this.maxCount && (ir >= 0 || iu >= 0))
					{
						var tr = 0, tu = 0;
						if (ir >= 0)
							tr = ar[ir].time;
						if (iu >= 0)
							tu = au[iu].time;
						
						if (tr > iu)
						{
							tabFoundRedirect.BlockInfo.push(ar[ir--]);
						}
						else
						{
							tabFoundRedirect.BlockInfo.push(au[iu--]);
						}

						cnt++;
					}
					
					tabFoundRedirect.BlockInfo.reverse();
					
				}
				else
				{
					tabFound.tabUri = ru;
					tabFound.redirected = true;
				}
			}
			else
				tabFound.redirected = false;
		}

	}
};
