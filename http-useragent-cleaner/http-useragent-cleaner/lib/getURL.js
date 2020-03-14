const {Cc, Ci, Cu, Cr, Cm, components} = require("chrome");
// console.error(require("sdk/url").DataURL('mainjs/urlFunction.js'));
Cu.import(require('sdk/self').data.url("mainjs/urlFunction.js"), exports);

exports.unload = 
function()
{
	Cu.unload(require('sdk/self').data.url('mainjs/urlFunction.js'));
};

exports.utils    = require("sdk/tabs/utils");
exports.viewFor  = require("sdk/view/core").viewFor;
exports.modelFor = require("sdk/model/core").modelFor;
exports.tabs     = require("sdk/tabs");

/* https://developer.mozilla.org/en-US/Add-ons/SDK/High-Level_APIs/tabs#Converting_to_XUL_tabs
var { modelFor } = require("sdk/model/core");
var { viewFor } = require("sdk/view/core");
*/

exports.isE10S = false;//require('sdk/preferences/service').get('browser.tabs.remote.autostart') || require('sdk/preferences/service').get('browser.tabs.remote.autostart.1') || require('sdk/preferences/service').get('browser.tabs.remote.autostart.2');

exports.isE10SA = require('sdk/preferences/service').get('browser.tabs.remote.autostart') || require('sdk/preferences/service').get('browser.tabs.remote.autostart.1') || require('sdk/preferences/service').get('browser.tabs.remote.autostart.2');

exports.sdkTabToChromeTab = function(sdkTab)
{
	var chromeTab  = this.viewFor(sdkTab);
	//var tabBrowser = this.utils.getBrowserForTab(chromeTab);
	
	return chromeTab;
},

exports.chromeTabToSdkTab = function(chromeTab)
{
	try
	{
		if (!chromeTab)
			return null;

		var sdkTab  = this.modelFor(chromeTab);
		//var tabBrowser = this.utils.getBrowserForTab(chromeTab);

		return sdkTab;
	}
	catch (e)
	{
		console.error('HUAC ERROR: chromeTabToSdkTab');
		console.error(e);
		console.error(chromeTab);

		return null;
	}

	return null;
},

exports.getBrowserForContext = function(context)
{
	// topFrameElement is the <browser> element
	var tfe = context.topFrameElement;

	if (!tfe && !this.isE10S)
	{
		try
		{
			return this.utils.getBrowserForTab(this.utils.getTabForContentWindow(context.associatedWindow));
		}
		catch (e)
		{
			// console.error(e);
		}

		return null;
	}
	
	return tfe;
};

exports.getTabForContext = function(context)
{
	try
	{
		return this.utils.getTabForContentWindow(context.associatedWindow);
	}
	catch (e)
	{
		console.error(e);

		return null;
	}
};


exports.getMessageManagerByContext = function(context)
{
	// topFrameElement is the <browser> element
	var tfe = context.topFrameElement;
	if (tfe)
	{
		return tfe.messageManager;
	}
	
	return null;
};

exports.getMessageManagerBySdkTab = function(tab)
{
	try
	{
		var chTab     = this.sdkTabToChromeTab(tab);
		var chBrowser = this.utils.getBrowserForTab(chTab);

		return chBrowser.messageManager;
	}
	catch (e)
	{
		console.error(e);
	}
};

exports.loadFrameScript = function(MessageManager, scriptUrl, async)
{
	MessageManager.loadFrameScript(scriptUrl, async);
};

// tab - ChromeTab
// urls.fromTabDocument(urls.sdkTabToChromeTab(tab)) для sdk/tab
// Вычисление url для вкладки по url документа, который в эту вкладку загружен.
// Т.к. url самой вкладки иногда изменяется произвольно с помощью history.pushState и history.replaceState.
// Этот url - совсем не тот, что отображается в адресной строке. Вплоть до того, что при том же origin это может быть другой домен
exports.fromTabDocument = function(tab, noHuac)
{
	try
	{
		if (!tab)
			return '';

		if (!noHuac && tab && tab.linkedBrowser && tab.linkedBrowser.huac)
		{
			if (!tab.linkedBrowser.huac.url)
			{
				if (!tab.linkedBrowser.huac.cpurl)
				{
					console.error('HUAC Error: !tab.linkedBrowser.huac.url');
					console.error(tab.linkedBrowser.huac);
				}

				return '';
			}

			return tab.linkedBrowser.huac.url;
		}

		var browser = this.utils.getBrowserForTab(tab);

		if (browser)
		{
			if (this.isE10SA)
			{
				if (browser._documentURI)
					return browser._documentURI.spec;
				else
					return '';
			}
			else
			{
				try
				{
					if (browser.contentWindow && browser.contentWindow.document)
					{
						if (!browser.contentWindow.document.URL)
						{
							return '';
						}

						return browser.contentWindow.document.URL;
					}
				}
				catch (e)
				{
					// console.error(e);
				}

				return '';
			}
		}
	}
	catch (e)
	{
		console.error(e);
	}

	/*
	// В случае, если вкладка пуста, идёт эта линия
	try
	{
		return this.utils.getURI(tab);
	}
	catch (e)
	{
	}
*/

	return null;
};

exports.fromBrowser = function(browser)
{
	if (!browser)
	{
		return '';
	}
	
	if (this.isE10SA)
	{
		if (browser._documentURI)
			return browser._documentURI.spec;
		else
			return '';
	}
	else
	{
		try
		{
			if (browser.huac)
				return browser.huac.url;

			if (browser.contentWindow && browser.contentWindow.document)
			{
				return browser.contentWindow.document.URL;
			}
		}
		catch (e)
		{
			// console.error(e);
		}

		return '';
	}
};
