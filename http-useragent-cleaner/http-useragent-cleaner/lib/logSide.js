const {Cc, Ci, Cu, Cr, Cm, components} = require("chrome");

exports.logger = function()
{
	this.tabs = [];
};

exports.logger.prototype =
{
	urls: require('./getURL'),

	CleanLog: function()
	{
		var tabs = this.urls.tabs;
		var I    = this.urls.utils;

		for (var ti in this.tabs)
		{
			var t = this.tabs[ti];
			if (!t.tabUri)
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
			this.tabs.splice(ti, 1);
		}
		// console.error(this.tabs);
	},
	
	CleanAll: function()
	{
		this.tabs = [];
	},
	
	FindTab: function(tabUrl)
	{
		if (!tabUrl || tabUrl == 'about:newtab' || tabUrl == 'about:blank' || tabUrl.indexOf('about:neterror') >= 0)
			tabUrl = '';

		if (tabUrl)
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

	maxCount: 1024,

	addToLog: function(tabUri, redirectTo, url, host, BlockInfo)
	{
		if (!this.enabled)
			return;

		var tabURISpec = host; //tabUri;

		if (tabURISpec)
		if (tabURISpec == 'about:blank' || tabURISpec == 'about:newtab' || tabURISpec.indexOf('about:neterror') >= 0)
			return;

		if (!tabUri)
			tabUri = '';
		
		if (!tabURISpec)
			tabURISpec = '';
/*
		if (tabURISpec)
		tabURISpec = this.urls.getURIWithoutScheme(tabURISpec.split('#')[0]);
*/
		if (redirectTo)
		{
			redirectTo = this.urls.getHostByURI(redirectTo);
		}

		var tabFound = this.FindTab(tabURISpec);

		if (!BlockInfo)
			BlockInfo = {};

		BlockInfo.url    = url;
		BlockInfo.taburl = this.urls.getURIWithoutScheme(tabUri.split('#')[0]);
		if (!tabFound)
		{
			if (redirectTo)
			{
				tabURISpec = redirectTo;
			}

			BlockInfo.time = Date.now();
			tabFound = {tabUri: tabURISpec, redirected: !!redirectTo, BlockInfo: [BlockInfo], startTime: Date.now()};
			this.tabs.push(tabFound);
		}
		else
		{
			BlockInfo.time = tabFound.BlockInfo[tabFound.BlockInfo.length-1].time;
			BlockInfo.repeatedCount = tabFound.BlockInfo[tabFound.BlockInfo.length-1].repeatedCount;

			var bi1 = JSON.stringify(BlockInfo);
			var bi2 = tabFound.BlockInfo.length > 0 ? JSON.stringify(tabFound.BlockInfo[tabFound.BlockInfo.length-1]) : '';

			BlockInfo.time = Date.now();
			BlockInfo.repeatedCount = 0;

			if (bi1 == bi2/* && BlockInfo.time - tabFound.BlockInfo[tabFound.BlockInfo.length-1].time < 5*1000*/)
			{
				if (tabFound.BlockInfo[tabFound.BlockInfo.length-1].repeatedCount)
					tabFound.BlockInfo[tabFound.BlockInfo.length-1].repeatedCount++;
				else
					tabFound.BlockInfo[tabFound.BlockInfo.length-1].repeatedCount = 1;
			}
			else
			if (tabFound.BlockInfo.length > 256)
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

			if (redirectTo)
			{
				var ru = redirectTo;
				var tabFoundRedirect = this.FindTab(ru);
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
/*
					for (var a of tabFound.BlockInfo)
						tabFoundRedirect.BlockInfo.push(a);

					if (tabFoundRedirect.BlockInfo.length > maxCount + 128)
						tabFoundRedirect.BlockInfo.splice(0, tabFoundRedirect.BlockInfo.length - maxCount - 128);
					else
					if (tabFoundRedirect.BlockInfo.length > maxCount)
						tabFoundRedirect.BlockInfo.splice(0, tabFound.BlockInfo.length);*/
					
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
