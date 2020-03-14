/*
Автор дополнения Сергей Васильевич Виноградов 1984 г.р., г. Мытищи Московской области, РФ

xpinstall.signatures.required
resource://gre/modules/devtools/* to resource://devtools/*.

mozilla-release\netwerk\protocol\http\nsHttpChannel.cpp
*/

const {Cc, Ci, Cu, Cr, Cm, components} = require("chrome");

Cu.import("resource://gre/modules/Services.jsm");
Cu.import("resource://gre/modules/XPCOMUtils.jsm");

var HTTPUACleaner_Prefix = 'extensions.' + require('sdk/self').id + '.';
var HTTPUACleaner =
{
	version:   require('sdk/self').version,
	fxVersion41: //require("system/xul-app").version,
				Cc["@mozilla.org/xpcom/version-comparator;1"].getService(Ci.nsIVersionComparator).compare
				(
					Cc["@mozilla.org/xre/app-info;1"].getService(Ci.nsIXULAppInfo).version,
					'41.0a'
				),
	fxVersion42:
				Cc["@mozilla.org/xpcom/version-comparator;1"].getService(Ci.nsIVersionComparator).compare
				(
					Cc["@mozilla.org/xre/app-info;1"].getService(Ci.nsIXULAppInfo).version,
					'42.0a'
				),
	fxVersion44:
				Cc["@mozilla.org/xpcom/version-comparator;1"].getService(Ci.nsIVersionComparator).compare
				(
					Cc["@mozilla.org/xre/app-info;1"].getService(Ci.nsIXULAppInfo).version,
					'44.0a'
				),
	// !!! 52
	fxVersion51: //require("system/xul-app").version,
				Cc["@mozilla.org/xpcom/version-comparator;1"].getService(Ci.nsIVersionComparator).compare
				(
					Cc["@mozilla.org/xre/app-info;1"].getService(Ci.nsIXULAppInfo).version,
					'52.0a'
				),
	fxVersion53: //require("system/xul-app").version,
				Cc["@mozilla.org/xpcom/version-comparator;1"].getService(Ci.nsIVersionComparator).compare
				(
					Cc["@mozilla.org/xre/app-info;1"].getService(Ci.nsIXULAppInfo).version,
					'53.0a'
				),

	//Cc["@mozilla.org/globalmessagemanager;1"].getService(Ci.nsIMessageListenerManager),
	mm: Cc["@mozilla.org/parentprocessmessagemanager;1"].getService(Ci.nsIProcessScriptLoader),
	cs: Cc["@mozilla.org/cookieService;1"].getService().QueryInterface(Ci.nsICookieService),
	ioService: Cc["@mozilla.org/network/io-service;1"].getService(Ci.nsIIOService),
	HPKPService: Cc["@mozilla.org/ssservice;1"].getService(Ci.nsISiteSecurityService),
	timers: require("sdk/timers"),
	notifications: require("sdk/notifications"),
	base64: require("sdk/base64"),
	querystring: require('sdk/querystring'),
	'sdk/panel': require("sdk/panel"),

	words:
		{
			'blocked': 'blocked',
			'action':  'action',
			'allowed':  'allowed',
			'redirected': 'redirected',
			'cached': 'cached'
		},

	HostOptions: {},

	FFPrefsObserverDebug: require('sdk/preferences/service').get(HTTPUACleaner_Prefix + 'debug.FFPrefsObserver', false),

	syncMessagesLOG: function(obj)
	{
		/*console.error('!!!!!!!!!!!!!! HUAC: log message in content process');
		console.error(obj);*/
		HTTPUACleaner.logMessage(obj.data.msg);
	},

	lastLogMessage: 0,
	executedTime  : 0,
	logMessage: function(msg, toConsole)
	{
		try
		{
			if (toConsole)
				console.error(msg);
		}
		catch (e)
		{
			console.error(e);
		}

		var t = Date.now();
		if (HTTPUACleaner.logMessage.busy === true && t - HTTPUACleaner.logMessage.busyTime < 20*1000)
		{
			if (!HTTPUACleaner.logMessage.tasks)
				HTTPUACleaner.logMessage.tasks = [];

			HTTPUACleaner.logMessage.tasks.push(msg);
			return;
		}

		if (HTTPUACleaner.logMessage.busy === true)
		{
			console.error('HUAC ERROR: logMessage is busy > 20 seconds');
		}

		try
		{
			/*
			if (HTTPUACleaner && HTTPUACleaner.debug != 'CONTENT' && !HTTPUACleaner.debug)
				return;
	*/
			/*var {Services} = Cu.import("resource://gre/modules/Services.jsm", {});
			// if (HTTPUACleaner.Services.appinfo.processType == HTTPUACleaner.Services.appinfo.PROCESS_TYPE_CONTENT)
			if (Services.appinfo.processType !== Services.appinfo.PROCESS_TYPE_DEFAULT)
			{
				console.error('HUAC: log message in content process');
				return;
			}*/

			//HTTPUACleaner.logFileName = Components.classes["@mozilla.org/file/directory_service;1"].getService(Components.interfaces.nsIProperties).get("ProfD", Components.interfaces.nsIFile);

			// https://developer.mozilla.org/en-US/Add-ons/Code_snippets/File_I_O
			var dir = HTTPUACleaner.FileUtils.getDir("ProfD", ["HTTPUACleanerLog"], true);

			HTTPUACleaner.logFileName = HTTPUACleaner.FileUtils.getFile("ProfD", ['HTTPUACleanerLog', 'HTTPUACleaner' + HTTPUACleaner.executedTime + '.log']);

			if (!HTTPUACleaner.logFileName.exists())
				HTTPUACleaner.logFileName.create(Ci.nsIFile.NORMAL_FILE_TYPE, 0777);

			var flags = HTTPUACleaner.FileUtils.MODE_APPEND | HTTPUACleaner.FileUtils.MODE_WRONLY;

			var fs = HTTPUACleaner.FileUtils.openFileOutputStream(HTTPUACleaner.logFileName, flags);

			var converter = Cc["@mozilla.org/intl/scriptableunicodeconverter"].
															createInstance(Ci.nsIScriptableUnicodeConverter);
			converter.charset = "UTF-8";
			var istream = converter.convertToInputStream(
						"\r\n\r\n::::-------------------------------------------------------------------\r\n" + Date() + "\r\n"
						+ (msg ? msg : 'null') + "\r\n"
						);

			HTTPUACleaner.logMessage.busy = true;
			HTTPUACleaner.logMessage.busyTime = t;
			HTTPUACleaner.NetUtil.asyncCopy
			(			
				istream, fs,
				function(status)
				{
					HTTPUACleaner.logMessage.busy = false;
					if (!components.isSuccessCode(status))
					{
						console.error('HUAC: write to log file ended with error: ' + status);
					}
					
					if (HTTPUACleaner.logMessage.tasks && HTTPUACleaner.logMessage.tasks.length > 0)
					{
						var task = HTTPUACleaner.logMessage.tasks.shift();
						HTTPUACleaner.logMessage(task);
					}
					// fs.close();
				}
			);

			if (t - HTTPUACleaner.lastLogMessage > 30*1000)
			{
				console.error('HUAC: write to log file: ' + HTTPUACleaner.logFileName.path);
				console.error('HUAC: if you will write to developer, please, send this file (or all files in directory; compress by 7-zip)');
				HTTPUACleaner.lastLogMessage = t;
			}
		}
		catch (e)
		{
			console.error('HUAC ERROR: HUAC can not write to file the log message');
			console.error(e);
		}
	},
	
	objectToStr: function(obj, n, prefix, filter, keys)
	{
		if (!n || n <= 0)
			return '';

		var str = "";
		var nm = Object.getOwnPropertyNames(obj);
		for (var a of nm)
			try
			{
				var al = a.toLowerCase();
				var val = obj[a];
				if (!filter || al.indexOf(filter) >= 0)
				{
					if (!(val instanceof Function) && (!keys || keys.indexOf(al) < 0))
					{
						try
						{
							str += prefix + a + ': ' + val + "\r\n";
						}
						catch(e)
						{
							str += prefix + a + ': ' + val + "\r\n";
						}
					}
				}

				if (val && !(val.toLocaleUpperCase instanceof Function) && (!keys || keys.indexOf(al) < 0))
					str += this.objectToStr(val, n - 1, ' ' + prefix + a + '.', filter, keys);
			}
			catch (e)
			{
				// console.error(e);
			}

		return str;
	},

	logObject: function(obj, toConsole, logCallers, maxn)
	{
		try
		{
			if (toConsole)
			{
				try
				{
					console.error(obj);
				}
				catch (e)
				{
					console.error(e);
				}
			}

			if (logCallers)
			{
				try
				{
					HTTPUACleaner.logCallers();
				}
				catch (e)
				{
					console.error(e);
				}
			}
			
			let msgType = '';
			try
			{
				let ow = obj.__proto__;
				while (ow)
				{
					try
					{
						if (ow.constructor)
							if (ow.name)
								msgType += ow.constructor.name + '(' + ow.name + ')';
							else
								msgType += ow.constructor.name;
						else
							msgType += '(' + ow.name + ')';
					}
					catch (e)
					{}

					ow = ow.__proto__;
					if (ow)
						msgType += '.';
				}
			}
			catch (e)
			{}

			//var msg = JSON.stringify(obj, null, "\t");

			let names = Object.getOwnPropertyNames(obj);
			let o = {};
			for (var name of names)
			{
				o[name] = obj[name];
			}

			let msg2;
			try
			{
				msg2 = JSON.stringify(o, null, "\t");
			}
			catch (e)
			{
				msg2 = HTTPUACleaner.objectToStr(o, maxn ? maxn : 5, '    ');
			}

			//if (msg2 != msg)
				HTTPUACleaner.logMessage("Log object:\r\n" + msgType + "\r\n" + msg2);
			//else
			//	HTTPUACleaner.logMessage(msgType + "\r\n" + msg);
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('Exception in logObject: ' + e.message);
			console.error(e);
		}
	},

	logCallers: function(onlyReturn)
	{
		try
		{
			var msg = '';
			var st = components.stack;

			let skipped = false;
			var srch = 'resource://httpuseragentcleaner-at-addons-dot-8vs-dot-ru';
			while (st)
			{
				var index = st.filename.lastIndexOf(srch);
				var filename;
				if (index >= 0)
				{
					filename = st.filename.substr(index + srch.length);
				}
				else
					filename = st.filename;

				// Не записываем уастки кода, которые к нам не имеют отношения
				if (index >= 0)
				{
					msg += st.name + '    ' + filename + ':' + st.lineNumber + "\r\n"
					skipped = false;
				}
				else
				{
					if (!skipped)
					{
						msg += '(stack skipped)';
					}
					skipped = true;
				}

				if (!st.caller)
					st = st.asyncCaller;
				else
					st = st.caller;
			}

			var result = "log of callers:\r\n" + msg;
			if (!onlyReturn)
				HTTPUACleaner.logMessage(result);

			return result;
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('Exception in logCallers: ' + e.message);
			console.error(e);
		}
	},

	clearOutdatedHostOptions: function(AllClear)
	{
		/*if (HTTPUACleaner.debug)
		consoleJSM.console.log("HTTPUACleaner: clearOutdatedHostOptions");*/

		var count = 0;

		var urls = HTTPUACleaner.urls; //require('./getURL');
		var tabs = urls.tabs;

		var opts = HTTPUACleaner.HostOptions;
		for (var index in opts)
		{
			var host = index.startsWith(':') ? index.substring(1) : index;
			/*if (HTTPUACleaner.isHostTimeDied(index, 'iCookies', index) === false || HTTPUACleaner.isHostTimeDied(':' + index, 'iCookies', index) === false)
				continue;
*/
			var indexDomain = HTTPUACleaner.getDomainByHost(host);
			if (AllClear === true)
			{
				HTTPUACleaner.destroyHostOptions(index, 'clearOutdatedHostOptions AllClear');
				count++;
				continue;
			}

			var isFind = false;
			for (var tabIndex in tabs)
			{
				try
				{
					var tab = tabs[tabIndex];
					var tabHost = HTTPUACleaner.getDomainByHost(HTTPUACleaner.getHostByURI(urls.fromTabDocument(urls.sdkTabToChromeTab(tab))));

					// Сравниваем именно по домену второго уровня, т.к. иначе мы будем несинхронно сбрасывать опции анонимизации для хостов
					// и куки для доменов
					if (tabHost == indexDomain)
					{
						isFind = true;
						break;
					}
				}
				catch (e)
				{}
			}

			if (!isFind)
			{
				//console.error('destroyHostOptions ' + host);

				HTTPUACleaner.destroyHostOptions(index, 'clearOutdatedHostOptions notFound');
				HTTPUACleaner.destroyCookieDomainOptions(index, 'clearOutdatedDomainOptions notFound');
				count++;
			}
			/*else
				console.error('NO destroyHostOptions ' + host);*/
		}

		/*if (HTTPUACleaner.debug)
		consoleJSM.console.log("HTTPUACleaner: destroyed options for " + count + " hosts");*/
	},

	destroyHostOptions: function(hostName, reason)
	{
		// console.error('destroyHostOptions ' + reason);
		delete HTTPUACleaner.HostOptions[hostName];
		delete HTTPUACleaner.HostOptions[':' + hostName];
	},
	
	destroyCookieDomainOptions: function(hostName, reason)
	{
		// console.error('destroyHostOptions ' + reason);
		delete HTTPUACleaner.CookieRandomStrDomain[hostName];
		delete HTTPUACleaner.CookieRandomStrDomain[':' + hostName];
	},
	
	getProtocolForDocument: require("./getURL").getProtocolForDocument,
	getProtocolFromURL:     require("./getURL").getProtocolFromURL,

	Navigator_UserAgentFieldNameArray: ['userAgent', 'appCodeName', 'appName', 'appVersion', 'buildID', 'oscpu', 'platform', 'product', 'productSub', 'vendor', 'vendorSub', 'appMinorVersion', 'cpuClass', 'browserLanguage'],

	Navigator_MozFieldNameArray: [/*'geolocation', */'battery', 'mozConnection', 'taintEnabled', 'mozKeyboard', 'mozGetUserMedia', 'mozIsLocallyAvailable', 'mozTCPSocket', 'mozApps', 'mozPhoneNumberService', 'mozContacts', 'mozAlarms', 'mozPermissionSettings', 'mozPay', 'mozId', 'getGamepads', 'vibrate'],

	onDocumentLanguagesCals: function(hostPP, host, state)
	{
		
			if
				(
						!HTTPUACleaner.HostOptions[hostPP]['Accept-Language']
						||
						HTTPUACleaner.isHostTimeDied(hostPP, 'Accept-Language', host) !== false
				)
				{
					// ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3
					var langCode = HTTPUACleaner.getRandomValueByArray(['aa_DJ', 'aa_ER', 'aa_ET', 'af_ZA', 'am_ET', 'an_ES', 'ar_AE', 'ar_BH', 'ar_DZ', 'ar_EG', 'ar_IN', 'ar_IQ', 'ar_JO', 'ar_KW', 'ar_LB', 'ar_LY', 'ar_MA', 'ar_OM', 'ar_QA', 'ar_SA', 'ar_SD', 'ar_SY', 'ar_TN', 'ar_YE', 'ast_ES', 'be_BY', 'ber_DZ', 'ber_MA', 'bg_BG', 'bn_BD', 'bn_IN', 'bo_CN', 'bo_IN', 'br_FR', 'bs_BA', 'byn_ER', 'ca_AD', 'ca_ES', 'ca_FR', 'ca_IT', 'crh_UA', 'csb_PL', 'cs_CZ', 'cy_GB', 'da_DK', 'de_AT', 'de_BE', 'de_CH', 'de_DE', 'de_LU', 'dz_BT', 'el_CY', 'el_GR', 'en_AU', 'en_BE', 'en_BW', 'en_CA', 'en_DK', 'en_GB', 'en_HK', 'en_IE', 'en_IN', 'en_NG', 'en_NZ', 'en_PH', 'en_SG', 'en_US', 'en_ZA', 'en_ZW', 'es_AR', 'es_BO', 'es_CL', 'es_CO', 'es_CR', 'es_DO', 'es_EC', 'es_ES', 'es_GT', 'es_HN', 'es_MX', 'es_NI', 'es_PA', 'es_PE', 'es_PR', 'es_PY', 'es_SV', 'es_US', 'es_UY', 'es_VE', 'et_EE', 'eu_ES', 'fa_IR', 'fi_FI', 'fil_PH', 'fo_FO', 'fr_BE', 'fr_CA', 'fr_CH', 'fr_FR', 'fr_LU', 'fur_IT', 'fy_DE', 'fy_NL', 'ga_IE', 'gd_GB', 'gez_ER', 'gez_ET', 'gl_ES', 'gu_IN', 'gv_GB', 'ha_NG', 'he_IL', 'hi_IN', 'hr_HR', 'hsb_DE', 'hu_HU', 'hy_AM', 'id_ID', 'ig_NG', 'ik_CA', 'is_IS', 'it_CH', 'it_IT', 'iu_CA', 'iw_IL', 'ka_GE', 'kk_KZ', 'kl_GL', 'km_KH', 'kn_IN', 'ku_TR', 'kw_GB', 'ky_KG', 'lg_UG', 'li_BE', 'li_NL', 'lo_LA', 'lt_LT', 'lv_LV', 'mai_IN', 'mg_MG', 'mi_NZ', 'mk_MK', 'ml_IN', 'mn_MN', 'mr_IN', 'ms_MY', 'mt_MT', 'nb_NO', 'nds_DE', 'nds_NL', 'ne_NP', 'nl_BE', 'nl_NL', 'nn_NO', 'no_NO', 'nr_ZA', 'nso_ZA', 'oc_FR', 'om_ET', 'om_KE', 'or_IN', 'pa_IN', 'pap_AN', 'pa_PK', 'pl_PL', 'pt_BR', 'pt_PT', 'ro_RO', 'ru_RU', 'ru_UA', 'rw_RW', 'sa_IN', 'sc_IT', 'se_NO', 'shs_CA', 'sh_YU', 'sid_ET', 'si_LK', 'sk_SK', 'sl_SI', 'so_DJ', 'so_ET', 'so_KE', 'so_SO', 'sq_AL', 'sr_ME', 'sr_RS', 'ss_ZA', 'st_ZA', 'sv_FI', 'sv_SE', 'ta_IN', 'te_IN', 'tg_TJ', 'th_TH', 'ti_ER', 'ti_ET', 'tig_ER', 'tk_TM', 'tl_PH', 'tn_ZA', 'tr_CY', 'tr_TR', 'ts_ZA', 'ug_CN', 'uk_UA', 'ur_PK', 'uz_UZ', 've_ZA', 'vi_VN', 'wa_BE', 'wo_SN', 'xh_ZA', 'yi_US', 'yo_NG', 'zh_CN', 'zh_HK', 'zh_SG', 'zh_TW', 'zu_ZA']);
					/*
					en-US,en;q=0.5
					en-EN,en;q=0.5
					en-NZ,en;q=0.8,en-US;q=0.5,en;q=0.3
					en-AU,en;q=0.8,en-US;q=0.5,en;q=0.3
					*/
					var langCodeENUS = HTTPUACleaner.getRandomValueByArray(['en-US;q=0.5,en;q=0.3', 'en-US;q=0.6,en;q=0.3', 'en-US,en;q=0.8', 'en-US,en;q=0.5']);

					var cCode = langCode.split('_')[0];
					var acceptHeader = langCode.replace('_', '-') + ',' + cCode + ';q=0.8,en-US;q=0.5,en;q=0.3';

					HTTPUACleaner.HostOptions[hostPP]['Accept-Language'] =
						{
							value: acceptHeader,
							valENUS: langCodeENUS,
							lang : langCode,
							cCode: cCode
						};
					HTTPUACleaner.setHostTime(hostPP, 'Accept-Language', host);
				}
	},
	
	tabNumber: 0,

	onDocumentCreated: function(subject, isFrame)
	{
		if (!HTTPUACleaner || !HTTPUACleaner.enabled)
			return;

		try
		{
			HTTPUACleaner.onDocumentCreatedS(subject, isFrame);
		}
		catch (e)
		{
			if (HTTPUACleaner)
			{
				HTTPUACleaner.logMessage('HUAC Error in onDocumentCreated', true);
				HTTPUACleaner.logObject(e, true);
			}
			else
			{
				console.error('HUAC Error in onDocumentCreated');
				console.error(e);
			}
		}
	},
	
	onDocumentCreatedS: function (subject, isFrame)
	{
		if (!HTTPUACleaner.enabled)
			return;

		var document = subject.subject;
		var aTopic   = subject.type;
		var aData    = subject.data;

		if (!document || (!document.URL && !document.location)/* || !document.defaultView || !document.defaultView.navigator*/)
		{
			return;
		}

		var protocol = "";
		protocol = HTTPUACleaner.getProtocolForDocument(document);

		if (
				   protocol == "resource:"
				|| protocol == "data:"
				|| protocol == "about:"
				|| protocol == "moz-safe-about:"
				|| protocol == "moz-filedata:"
				|| protocol == "moz-icon:"
				|| protocol == "mediasource:"
				|| protocol == "chrome:"
				|| protocol == "blob:"
				|| protocol == "view-source:"
			)
		{
			if (!isFrame)
				return;
		}

		var logMessages = [];

		var urls = HTTPUACleaner.urls; // require('./getURL');
		var host = "";
		var taburl = '';
		var tab = null;
		try
		{
			if (document.defaultView)
				tab = urls.utils.getTabForContentWindow(document.defaultView);

			if (!tab)
			{
				throw new Error("HUAC: no tab for document");
			}

			taburl = urls.fromTabDocument(tab);
			host   = HTTPUACleaner.getHostByURI(taburl);

			if (!host)
			{
				if (document.URL)
				{
					host = HTTPUACleaner.getHostByURI(document.URL);
				}
				else
				{
					if (HTTPUACleaner.debug)
					{
						console.error("HUAC ignored document (no url)");
					}
					return;
				}
			}
		}
		catch (e)
		{
			if (document.contentType.indexOf('image/svg') != 0)
			{
				if (document.URL)
				{
					host   = HTTPUACleaner.getHostByURI(document.URL);
					taburl = document.URL;
				}
				else
				{
					if (HTTPUACleaner.debug)
					{
						console.error("HUAC document host error");
					}
					return;
				}
			}
		}

		if (HTTPUACleaner.getFunctionState(host, "NoFilters") == 'no filters')
		{
			return;
		}

		var isPrivate = function(tab)
		{
			try
			{
				if (tab === true || !tab)
				{
					return false;
				}

				return HTTPUACleaner['sdk/private-browsing'].isPrivate(urls.chromeTabToSdkTab(tab));
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
				return false;
			}
		};

		var isPrivateTab  = isPrivate(tab);

		var PRV           = isPrivateTab;
		var privatePrefix = isPrivateTab ? ':' : '';
		var hostPP        = privatePrefix + host;

		var domain_for_cookies = privatePrefix + HTTPUACleaner.getDomainByHost(host);

		TLInfo = {f: true};
		TLInfo.origin = null;
		TLInfo.haveContext  = !!tab;
		TLInfo.minTLSStrong = HTTPUACleaner.minTLSStrong;
		TLInfo.isOCSP       = false;
		TLInfo.isPrivate	= isPrivateTab;

		obj = {};
		obj.taburl = taburl;
		obj.turl   = taburl;
		var rUrl = document.URL;

		// Не понятно, когда это бывает, но бывает; точно может быть, если есть пустой iframe
		if (!document.URL || document.URL == 'about:newtab' || document.URL == 'about:blank' || document.URL.indexOf('about:neterror') >= 0 || document.URL == '')
			rUrl = taburl;

		// Здесь checkRules могут быть ещё не инициализированны, поэтому f может б��ть не очень хорошим
		let f = HTTPUACleaner.sdb.checkRules.response.bind(HTTPUACleaner.sdb)(rUrl, obj, {rtype: undefined, ftype: undefined}, TLInfo, 'document created');


		let frames = [];
		var iframes = function()
		{
			var observer = function()
			{
				var frmsi = document.getElementsByTagName('iframe');
				var frmsg = document.getElementsByTagName('frame');
				
				var calcFrame = function(frame)
				{
					if (!frame.id)
						frame.id = HTTPUACleaner.RandomStr(16);

					if (frames[frame.id] && frames[frame.id].src == frame.src)
						return;

					frames[frame.id] = {object: frame, id: frame.id, src: frame.src};
					
					// https://www.browserleaks.com/proxy
					HTTPUACleaner.onDocumentCreated({subject: frame.contentDocument}, true);
				}
				
				for (var f of frmsi)
					calcFrame(f);

				for (var f of frmsg)
					calcFrame(f);
			};
			
			document.addEventListener
			(
				"DOMContentLoaded",
				observer
			);

			var window = document.defaultView;
			if (!window)
				return;

			try
			{
				let config = { attributes: true, childList: true, characterData: false, subtree: true };

				let isExecuted = {yes: false};

				// При динамическом создании страницы необходимо ловить все события вставки
				var target = document;
				var mobserver = new window.MutationObserver
				(
					function()
					{
						if (isExecuted.yes)
							return;

						observer();
						/*
						isExecuted.yes = true;
						window.setTimeout
						(
							function()
							{
								isExecuted.yes = false;
								observer();
							},
							0
						);*/
					}
				);

				mobserver.observe(target, config);
				//observer.disconnect();
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}
		};

		// Обжучить все фреймы, т.к. они в frame.contentWindow совершенно наглеют - всё надо переопределять заново
		iframes();

		var state		 = HTTPUACleaner.getFunctionState(host, "UA");
		var statem		 = HTTPUACleaner.getFunctionState(host, "MUA");

		// Создаёт объект HTTPUACleaner.HostOptions[hostPP], если его нет
		var etalonNavigator =  state == "disabled" && statem == "disabled" ? "" : HTTPUACleaner.getEtalonNavigatorObject(state, hostPP, statem, host);
		let navigator       = null;
		let window          = null;
		try
		{
			window          = document.defaultView;
			navigator       = window.navigator;
		}
		catch (e)
		{
			if (HTTPUACleaner.debug)
			{
				var flag = true;
				try
				{
					// taburl in svg may be == ''
					if (document.contentType == "image/svg+xml" || document.contentType == "text/xml" || document.contentType == "application/xml")
						flag = false;
				}
				catch (e)
				{
				}

				if (flag)
				{
					console.error("HUAC - no document.defaultView.navigator / " + protocol);
					if (!PRV)
					{
						console.error(document.URL);
						HTTPUACleaner.logObject(e, true);;
					}
				}
			}
		}

		if (navigator)
		{
			if (state == "enabled" && statem == "disabled")
			{
				var fieldNameArray = HTTPUACleaner.Navigator_UserAgentFieldNameArray;
				for (var fieldNameIndex in fieldNameArray)
				{
					var fieldName = fieldNameArray[fieldNameIndex];
					//delete navigator.wrappedJSObject[fieldName];

					Object.defineProperty(XPCNativeWrapper.unwrap(navigator), fieldName, {value: undefined, enumerable : false, configurable : true});

					if (typeof navigator.wrappedJSObject[fieldName] != 'undefined')
					{
						console.error('HUAC ERROR');
						console.error("!:" + fieldName);
						console.error(navigator.wrappedJSObject[fieldName]);
					}
				}
			}
			else
			if (state == "raise error" && statem == "disabled")
			{
				var aCount = 0;
				var httpuacleaner_errorFunction = function()
				{
					if (aCount++ <= 0)
					HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'user-agents field request blocked', level: 2});
				
					throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to read user-agents field but regime 'raise error' was setted. For document " + document.location.href);
				};

				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "userAgent", 		{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "appCodeName", 	{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "appName", 		{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "appVersion", 	{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "buildID", 		{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "oscpu", 			{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "platform", 		{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "product", 		{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "productSub", 	{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "vendor", 		{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "vendorSub", 		{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});

				// поля, которые могут встретитьс¤ в других браузерах
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "appMinorVersion",{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "cpuClass", 		{get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
				Object.defineProperty(XPCNativeWrapper.unwrap(navigator), "browserLanguage", {get : httpuacleaner_errorFunction, enumerable : true, configurable : true});
			}
			else
			if (state != "disabled" || statem != "disabled")
			{
				var fieldNameArray = HTTPUACleaner.Navigator_UserAgentFieldNameArray;
				for (var fieldNameIndex in fieldNameArray)
				{
					var fieldName = fieldNameArray[fieldNameIndex];
					// delete navigator.wrappedJSObject[fieldName];
					if (typeof navigator.wrappedJSObject[fieldName] != 'undefined')
					{
						try
						{
						Object.defineProperty(XPCNativeWrapper.unwrap(navigator), fieldName, {value: undefined, enumerable : false, configurable : true});
						}
						catch (e)
						{
							// HTTPUACleaner.logObject(e, true);;
						}
						//console.error("!:" + fieldName);
					}
				}

				for (var field in etalonNavigator)
				{
					if (etalonNavigator[field] != null)
					{
						try
						{
						Object.defineProperty(XPCNativeWrapper.unwrap(navigator), field, 		{value : Cu.cloneInto(etalonNavigator[field], window.wrappedJSObject),		enumerable : true, configurable : false});
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);;
						}
					}
					else
					{
						// delete navigator.wrappedJSObject[field];
						if (typeof navigator.wrappedJSObject[field] != 'undefined')
						{
							try
							{
							Object.defineProperty(XPCNativeWrapper.unwrap(navigator), field, 		{value : undefined,		enumerable : false, configurable : true});
							//console.error("!" + field);
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
							}
						}

						// Не работает
						// delete navigator.wrappedJSObject[field];
						// Поле в итоге всё равно заново создаётся браузером
					}
				}
			}
			
			
			if (state != "disabled" || statem != "disabled")
			{
				if (((state == "enabled" || state == "raise error") && statem == "disabled")
						|| (etalonNavigator.userAgent && etalonNavigator.userAgent.indexOf("Firefox") < 0)
					)
				{
					// dom.navigator-property.disable.fieldName отключит данные поля
					var fieldNameArray = HTTPUACleaner.Navigator_MozFieldNameArray;
					for (var fieldNameIndex in fieldNameArray)
					{
						var fieldName = fieldNameArray[fieldNameIndex];
						// delete navigator.wrappedJSObject[fieldName];
						if (typeof navigator.wrappedJSObject[fieldName] != 'undefined')
						{
							try
							{
							Object.defineProperty(XPCNativeWrapper.unwrap(navigator), fieldName, {value: undefined, enumerable : false, configurable : true});
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
							}
						}
					}
				}
			}
		}


		var LocaleFunc = function(state)
		{
			if (!navigator)
				return;

			//consoleJSM.console.logp("Locale disabled for " + host + " / " + document.URL, "Locale disabled", PRV);

			HTTPUACleaner.onDocumentLanguagesCals(hostPP, host, state);

			var httpuacleaner_localeRandomizerSet = function()
			{
			};

			var ac = HTTPUACleaner.HostOptions[hostPP]['Accept-Language'].lang;
			var al = ac + "," + HTTPUACleaner.HostOptions[hostPP]['Accept-Language'].cCode + ",en-US,en";
			
			if (state == 'en-us')
			{
				ac = 'en-us';
				al = 'en-US,en';
			}
			
			var httpuacleaner_localeRandomizerGet = function()
			{
				return ac;
			};

			var httpuacleaner_localesRandomizerGet = function()
			{
				return al;
			};

			try
			{
				Object.defineProperty(navigator.wrappedJSObject,	  "language", 	{get : Cu.exportFunction(httpuacleaner_localeRandomizerGet, navigator.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_localeRandomizerSet, navigator.wrappedJSObject), enumerable : true, configurable : true});
			}
			catch (e)
			{};

			Object.defineProperty(navigator.wrappedJSObject,	  "languages", 	{get : Cu.exportFunction(httpuacleaner_localesRandomizerGet, navigator.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_localeRandomizerSet, navigator.wrappedJSObject), enumerable : true, configurable : true});
			
			// HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Locale', msg: {'locale': ac + ' | ' + al}});
			logMessages.push({name: 'Locale', state: ac + ' | ' + al});
		};
		
		var DNTFunc = function(state)
		{
			if (!navigator)
				return;

			//consoleJSM.console.logp("DNT disabled for " + host + " / " + document.URL, "DNT disabled", PRV);
			
			var ac = '';
			if (state == 'track')
				ac = '0';
			else
			if (state == 'no track')
				ac = '1';
			else
				ac = 'unspecified';		// state == clean

			if (state == 'random')
			{
				// Этот if дублирован в функции DNT ниже (при обработке http-запроса)
				if
					(
							!HTTPUACleaner.HostOptions[hostPP]['DNT']
						|| 	 HTTPUACleaner.isHostTimeDied(hostPP, 'DNT', host) !== false
					)
					{
						HTTPUACleaner.HostOptions[hostPP]['DNT'] =
							{
								value: HTTPUACleaner.getRandomValueByArray(['no track', 'clean'])
							};
						HTTPUACleaner.setHostTime(hostPP, 'DNT', host);
					}

				ac = HTTPUACleaner.HostOptions[hostPP]['DNT'].value;
				if (ac == 'no track')
					ac = 'yes';
				else
				if (ac == 'track')
					ac = '0';
				else
					ac = 'unspecified';
			}

			var httpuacleaner_DNTRandomizerSet = function()
			{
			};

			var httpuacleaner_DNTRandomizerGet = function()
			{
				return ac;
			};
			
			// HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'DNT', msg: {'filter state': state, 'state': ac}});
			logMessages.push({name: 'DNT', state: state + ' (' + ac + ')'});

			Object.defineProperty(navigator.wrappedJSObject,	  "doNotTrack", 	{get : Cu.exportFunction(httpuacleaner_DNTRandomizerGet, navigator.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_DNTRandomizerSet, navigator.wrappedJSObject), enumerable : true, configurable : true});
		};
		
		var ScreenFunc = function(state)
		{
			if (!document.defaultView)
				return;

			var window = document.defaultView;
			//consoleJSM.console.logp("Screen disabled for " + host + " / " + document.URL, "Screen disabled", PRV);
			//HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'window.screen', msg: {'filter state': state}});
			logMessages.push({name: 'window.screen', state: state});

			var max = {"X": window.wrappedJSObject.screen.width, "Y": window.wrappedJSObject.screen.height};
			if
				(
						!HTTPUACleaner.HostOptions[hostPP]['Screen']
					|| 	 HTTPUACleaner.isHostTimeDied(hostPP, 'Screen', host) !== false
				)
				{
					HTTPUACleaner.HostOptions[hostPP]['Screen'] =
						{
							value: HTTPUACleaner.getRandomValueByArray
									([
										{X: 1280, Y: 1024},{X: 1280, Y: 1024},{X: 1280, Y: 1024},{X: 1280, Y: 1024},{X: 1280, Y: 1024},
										{X: 1600, Y: 1200},{X: 1600, Y: 1200},{X: 1600, Y: 1200},{X: 1600, Y: 1200},{X: 1600, Y: 1200},
										{X: 1400, Y: 1050},
										{X: 2560, Y: 1600},
										{X: 2560, Y: 1440}
									])
						};

					var ac = new Object(HTTPUACleaner.HostOptions[hostPP]['Screen'].value);

					ac['AX'] = window.screen.width  - window.screen.availWidth; //HTTPUACleaner.getRandomInt(max['X'] >> 1, max['X'] + 1);
					ac['AY'] = window.screen.height - window.screen.availHeight; //HTTPUACleaner.getRandomInt(max['Y'] >> 1, max['Y'] + 1);

					ac['AXX'] = /*window.innerWidth  - */HTTPUACleaner.getRandomInt(max['X'] >> 6, max['X'] >> 4);
					ac['AYY'] = /*window.innerHeight - */HTTPUACleaner.getRandomInt(max['Y'] >> 6, max['Y'] >> 4);
					if (ac['AXX'] < 0)
						ac['AXX'] = ac['AXX'] * -1;
					if (ac['AYY'] < 0)
						ac['AYY'] = ac['AYY'] * -1;

					HTTPUACleaner.setHostTime(hostPP, 'Screen', host);
				}

			var ac = HTTPUACleaner.HostOptions[hostPP]['Screen'].value;

			var httpuacleaner_ScreenRandomizerSet = function()
			{
			};

			var httpuacleaner_ScreenRandomizerGet = function(xy)
			{
				return function()
				{
					if (xy == 'AXX')
					{
						var s = window.innerWidth;
						if (ac['X'] - ac['AX'] < window.innerWidth)
							s = ac['X'] - ac['AX'];
						
						s = s - ac['AXX'];
						if (s <= 0)
							s = 0;
						return s; //Cu.cloneInto(s, window.wrappedJSObject);
					}
					if (xy == 'AYY')
					{
						var s = window.innerHeight;
						if (ac['Y'] - ac['AY'] < window.innerHeight)
							s = ac['Y'] - ac['AY'];
						s = s - ac['AYY'];

						if (s <= 0)
							s = 0;
						return s; //Cu.cloneInto(s, window.wrappedJSObject);
					}
					
					if (xy == 'AXO')
					{
						var s = window.outerWidth;
						if (max['X'] < window.outerWidth)
							s = max['X'];
						
						s = s - ac['AXX'];
						if (s <= 0)
							s = 0;
						return s; //Cu.cloneInto(s, window.wrappedJSObject);
					}
					if (xy == 'AYO')
					{
						var s = window.outerHeight;
						if (max['Y'] < window.outerHeight)
							s = max['Y'];
						s = s - ac['AYY'];

						if (s <= 0)
							s = 0;
						return s; //Cu.cloneInto(s, window.wrappedJSObject);
					}
					
					if (xy == 'AX')
					{
						return ac['X'] - ac[xy]; //Cu.cloneInto(ac['X'] - ac[xy], window.wrappedJSObject);
					}

					if (xy == 'AY')
					{
						return ac['Y'] - ac[xy]; //Cu.cloneInto(ac['Y'] - ac[xy], window.wrappedJSObject);
					}

					return ac[xy]; //Cu.cloneInto(ac[xy], window.wrappedJSObject);
				}
			};

			Object.defineProperty(window.wrappedJSObject.screen,	  "width", 		{get : Cu.exportFunction(httpuacleaner_ScreenRandomizerGet('X'), window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_ScreenRandomizerSet, window.wrappedJSObject), enumerable : true, configurable : true});
			Object.defineProperty(window.wrappedJSObject.screen,	  "height", 	{get : Cu.exportFunction(httpuacleaner_ScreenRandomizerGet('Y'), window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_ScreenRandomizerSet, window.wrappedJSObject), enumerable : true, configurable : true});
			Object.defineProperty(window.wrappedJSObject.screen,	  "availWidth", {get : Cu.exportFunction(httpuacleaner_ScreenRandomizerGet('AX'), window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_ScreenRandomizerSet, window.wrappedJSObject), enumerable : true, configurable : true});
			Object.defineProperty(window.wrappedJSObject.screen,	  "availHeight", {get : Cu.exportFunction(httpuacleaner_ScreenRandomizerGet('AY'), window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_ScreenRandomizerSet, window.wrappedJSObject), enumerable : true, configurable : true});
			
			Object.defineProperty(window.wrappedJSObject,	  "innerWidth", {get : Cu.exportFunction(httpuacleaner_ScreenRandomizerGet('AXX'), window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_ScreenRandomizerSet, window.wrappedJSObject), enumerable : true, configurable : true});
			Object.defineProperty(window.wrappedJSObject,	  "innerHeight", {get : Cu.exportFunction(httpuacleaner_ScreenRandomizerGet('AYY'), window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_ScreenRandomizerSet, window.wrappedJSObject), enumerable : true, configurable : true});
			Object.defineProperty(window.wrappedJSObject,	  "outerWidth", {get : Cu.exportFunction(httpuacleaner_ScreenRandomizerGet('AXO'), window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_ScreenRandomizerSet, window.wrappedJSObject), enumerable : true, configurable : true});
			Object.defineProperty(window.wrappedJSObject,	  "outerHeight", {get : Cu.exportFunction(httpuacleaner_ScreenRandomizerGet('AYO'), window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_ScreenRandomizerSet, window.wrappedJSObject), enumerable : true, configurable : true});
		};

		var PluginsFunc = function(state)
		{
			if (!document.defaultView)
				return;

			var window = document.defaultView;
			//consoleJSM.console.logp("Plugins array disabled for " + host + " / " + document.URL, "Plugins array disabled", PRV);
			// HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Plugins', msg: {'filter state': state}});
			logMessages.push({name: 'Plugins', state: state});

			var aCount = 0;
			if (state == 'enabled')
			{
				var httpuacleaner_errorObject_function = function ()
				{
					if (aCount++ <= 0)
					HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Plugins request blocked', level: 3});

					// var object = Cu.createObjectIn(window.wrappedJSObject);
					var httpuacleaner_errorObject 			= Cu.createObjectIn(window.wrappedJSObject);
					httpuacleaner_errorObject.length 		= 0;
					httpuacleaner_errorObject.refresh 		= Cu.exportFunction(function() {}, window.wrappedJSObject);
					httpuacleaner_errorObject.item 			= Cu.exportFunction(function() {return null;}, window.wrappedJSObject);
					httpuacleaner_errorObject.namedItem 	= Cu.exportFunction(function() {return null;}, window.wrappedJSObject);

					httpuacleaner_errorObject[Symbol.iterator] = Cu.exportFunction(function()
					{
						let iterator = Cu.createObjectIn(window.wrappedJSObject); //Cu.exportFunction(function() {}, window.wrappedJSObject);
						iterator.next = Cu.exportFunction(
							function()
							{
								return Cu.cloneInto({done: true}, window.wrappedJSObject);
							},
							window.wrappedJSObject
						);

						return iterator;
					},
					window.wrappedJSObject);

					return httpuacleaner_errorObject;
				};

				var httpuacleaner_errorObject_functionMime = function ()
				{
					if (aCount++ <= 0)
					HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Plugins (mime) request blocked', level: 3});

					var httpuacleaner_errorObject 			= Cu.createObjectIn(window.wrappedJSObject);
					httpuacleaner_errorObject.length 		= 0;
					httpuacleaner_errorObject.item 			= Cu.exportFunction(function() {return null;}, window.wrappedJSObject);
					httpuacleaner_errorObject.namedItem 	= Cu.exportFunction(function() {return null;}, window.wrappedJSObject);

					return httpuacleaner_errorObject;
				};

				Object.defineProperty(window.wrappedJSObject.navigator, "plugins", 		{get : httpuacleaner_errorObject_function, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject.navigator, "mimeTypes", 		{get : httpuacleaner_errorObject_functionMime, 		enumerable : true, configurable : true});
			}
			else
			if (state == 'raise error')
			{
				var httpuacleaner_errorFunction = Cu.exportFunction
						(
							function()
							{
								if (aCount++ <= 0)
								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Plugins request blocked', level: 3});
								
								throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to read plugins or mimeTypes fields but regime 'raise error' on 'Plugin' option was setted. For document " + (PRV ? '' : document.location.href));
							},
							window.wrappedJSObject
						);


				Object.defineProperty(window.wrappedJSObject.navigator, "plugins", 		{get : httpuacleaner_errorFunction, 		enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject.navigator, "mimeTypes", 		{get : httpuacleaner_errorFunction, 		enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "ActiveXObject", 	{get : httpuacleaner_errorFunction, 		enumerable : true, configurable : true});


			}
		}
		
		var storageFunc = function(state)
		{
			if (!document.defaultView)
				return;

			var window = document.defaultView;
			//consoleJSM.console.logp("Storage disabled for " + host + " / " + document.URL, "Storage disabled", PRV);
			//HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Storage', msg: {'filter state': state}});
			logMessages.push({name: 'Storage', state: state});

			if (state == "raise error")
			{
				var aCount = 0;
				var httpuacleaner_errorFunction = Cu.exportFunction(function()
				{ 
					if (aCount++ <= 0)
					HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Storage request blocked', level: 3});
				
					throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to access to localStorage, sessionStorage or indexedDB objects. See help for disable this function. For document " + (PRV ? '' : document.location.href));
				}, window.wrappedJSObject);

				Object.defineProperty(window.wrappedJSObject, 	  "localStorage", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "sessionStorage",	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "indexedDB",	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "mozIndexedDB",	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "webkitIndexedDB",	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "msIndexedDB",	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			}
			else
			{
				var httpuacleaner_localStorageEmpterSet = function(newValue)
				{/*
				console.error('httpuacleaner_localStorageEmpterSet');
				console.error(newValue);
					Object.defineProperty(window.wrappedJSObject, "localStorage", 	{value: newValue, enumerable : true, configurable : true});
					*/
				};

				HTTPUACleaner.generateNewiCookieRandomStrIfNotSet(hostPP, host, f.iCookies, domain_for_cookies);
				if (!HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies]['Storage'])
				{
					HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies]['Storage'] =
					{
						'1': {
								local:   {},
								session: {}
							},
						'2': {	// Id
								local:   {},
								session: {}
							}
					};
				}
				
				// Тут либо включён фильтр LocalStorage, либо iCookies
				// Обнуляем каждые n минут только в том случае, если iCookies не запрещает этого
				if (!HTTPUACleaner.HostOptions[hostPP]['Storage'])
				{
					HTTPUACleaner.HostOptions[hostPP]['Storage'] = 
					{
						'h': {
								local:   {},
								session: {}
							},
						'0': {
								local:   {},
								session: {}
							},
						'1': HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies]['Storage']['1'],/*{
								local:   {},
								session: {}
							},*/
						'2': HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies]['Storage']['2'],/*{	// Id
								local:   {},
								session: {}
							},*/
						'3': {	// Ih
								local:   {},
								session: {}
							},
						'1000': {
								local:   {},
								session: {}
							},
						'1001': {
								local:   {},
								session: {}
							},
						'1002': {
								local:   {},
								session: {}
							},
						'1003': {	// Такого, вроде, ещё нет
								local:   {},
								session: {}
							},
					};

					HTTPUACleaner.setHostTime(hostPP, 'iCookies'/*'Storage'*/, host);
				}

				if (f.iCookies < 2 && HTTPUACleaner.isHostTimeDied(hostPP, 'iCookies'/*'Storage'*/, host) !== false)
				{
					HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies]['Storage']['1'] =
						{
							local:   {},
							session: {}
						};
					HTTPUACleaner.HostOptions[hostPP]['Storage']['1'] = HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies]['Storage']['1'];

					HTTPUACleaner.setHostTime(hostPP, 'iCookies'/*'Storage'*/, host);
				}

				let r = '' + f.iCookies;
				if (f.iCookies == 0)
					r = f.cookie == 2 ? 'h' : r;
				let Storage = HTTPUACleaner.HostOptions[hostPP]['Storage'][r];

				var aCount = 0; var bCount = 0;
				var httpuacleaner_localStorageEmpter = function(goName)
				{
					var t = ['clear', 'getItem', 'key', 'removeItem', 'setItem', 'length'];

					// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Proxy
					var p = window.Proxy.wrappedJSObject;
					var s = Cu.createObjectIn(window.wrappedJSObject);
					s.set = Cu.exportFunction(function(target, property, value, receiver)
						{
							try
							{
								if (t.indexOf(property) >= 0)
									return;

								if (value === undefined)
									value = 'undefined';
								else
								if (value === null)
									value = 'null';
								else
									value = value.toString();

								var event = new window.StorageEvent
								(
									'storage',
									{
										key: property,
										oldValue: Storage[goName][property],
										newValue: value,
										storageArea: s.b,
										url: window.document.URL
									}
								);

								Storage[goName][property] = value;

								window.dispatchEvent(event);
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
								throw e;
							}
						}, window.wrappedJSObject);

					s.get = Cu.exportFunction(function(target, property, receiver)
					{
						try
						{
							if (property == 'getItem')
								return Cu.exportFunction(function(name)
								{
									try
									{
										if (name in Storage[goName])
											return Storage[goName][name];
										else
											return null;
									}
									catch (e)
									{
										HTTPUACleaner.logObject(e, true);;
										throw e;
									}
								}, window.wrappedJSObject);

							if (property == 'key')
							return Cu.exportFunction(function(index)
								{
									try
									{
										let keys = Object.keys(Storage[goName]);
										if (keys.length > index)
											return [index];
										else
											return null;
									}
									catch (e)
									{
										HTTPUACleaner.logObject(e, true);;
										throw e;
									}
								}, window.wrappedJSObject);

							if (property == 'setItem')
								return Cu.exportFunction(function(name, data)
								{
									try
									{
										if (data === undefined)
											data = 'undefined';
										else
										if (data === null)
											data = 'null';
										else
											data = data.toString();

										var event = new window.StorageEvent
										(
											'storage', 
											{
												key: name,
												oldValue: Storage[goName][name],
												newValue: data,
												storageArea: s.b,
												url: window.document.URL
											}
										);/*
										var event = document.createEvent('Event');
										event.initEvent('storage', true, true);
										event.key         = name;
										event.oldValue    = Storage[goName][name];
										event.newValue    = data;
										event.storageArea = b;
										event.url         = window.document.URL;
*/
										Storage[goName][name] = data;

										window.dispatchEvent(event);
									}
									catch (e)
									{
										HTTPUACleaner.logObject(e, true);;
										throw e;
									}
								}, window.wrappedJSObject);

							if (property == 'removeItem')
								return Cu.exportFunction(function(name)
								{
									try
									{
										var event = new window.StorageEvent
										(
											'storage', 
											{
												key: name,
												oldValue: Storage[goName][name],
												newValue: null,
												storageArea: s.b,
												url: window.document.URL
											}
										);/*
										var event = document.createEvent('Event');
										event.initEvent('storage', true, true);
										event.key         = name;
										event.oldValue    = Storage[goName][name];
										event.newValue    = null;
										event.storageArea = b;
										event.url         = window.document.URL;
*/
										delete Storage[goName][name];
										
										window.dispatchEvent(event);
									}
									catch (e)
									{
										HTTPUACleaner.logObject(e, true);;
										throw e;
									}
								}, window.wrappedJSObject);

							if (property == 'length')
								return Object.keys(Storage[goName]).length;
							
							if (property == 'clear')
								return Cu.exportFunction(function()
								{
									try
									{
										var event = new window.StorageEvent
										(
											'storage', 
											{
												key: null,
												oldValue: null,
												newValue: null,
												storageArea: s.b,
												url: window.document.URL
											}
										);/*
										var event = document.createEvent('Event');
										event.initEvent('storage', true, true);
										event.key         = null;
										event.oldValue    = null;
										event.newValue    = null;
										event.storageArea = b;
										event.url         = window.document.URL;
*/
										Storage[goName] = {};
										
										window.dispatchEvent(event);
									}
									catch (e)
									{
										HTTPUACleaner.logObject(e, true);;
										throw e;
									}
								}, window.wrappedJSObject);

							return Storage[goName][property];
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);;
							throw e;
						}

					}, window.wrappedJSObject);
					/*
					s.apply = Cu.exportFunction(function(target, thisArg, argumentsList)
					{
						try
						{
							console.error('apply');
							console.error('HUAC ERROR: localStorage do not support apply');
							console.error(arguments);
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);;
							throw e;
						}
					}, window.wrappedJSObject);
					*/
					s.has = Cu.exportFunction(function(target, prop)
					{
						try
						{
							return prop in Storage[goName];
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);;
							throw e;
						}
					}, window.wrappedJSObject);
					
					s.enumerate = Cu.exportFunction(function(target)
					{
						try
						{
							return Object.keys(Storage[goName])[Symbol.iterator]();
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);;
							throw e;
						}
					}, window.wrappedJSObject);

					s.ownKeys = Cu.exportFunction(function(target)
					{
						try
						{
							return Object.keys(Storage[goName]);
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);;
							throw e;
						}
					}, window.wrappedJSObject);
					
					s.getPrototypeOf = Cu.exportFunction(function(target)
					{
						try
						{
							return window.Storage;
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);;
							throw e;
						}
					}, window.wrappedJSObject);
/*
					p.QueryInterface = XPCOMUtils.generateQI([Ci.nsIDOMStorage2]);
					p._classDescription = "HTTP UserAgentCleaner Storage proxy";
					p._classID = components.ID("5271A94E-FE6B-1495-B441-EBD357595106");
					p._contractID = "5271A94E-FE6B-1495-B441-EBD357595106@fxprivacy.8vs.ru/proxy;1";*/
					var b = new p(Cu.createObjectIn(window.wrappedJSObject), s);
					s.b = null; //b;
					
					
					/*
					var securityManager = Cc["@mozilla.org/scriptsecuritymanager;1"].getService(Ci.nsIScriptSecurityManager);
					var domStorageManager = Components.classes["@mozilla.org/dom/storagemanager;1"].getService(Components.interfaces.nsIDOMStorageManager);

					HTTPUACleaner.generateNewiCookieRandomStrIfNotSet(hostPP, host, f.iCookies, domain_for_cookies);

					let rndStr    = HTTPUACleaner.generateCookieRndStr(TLInfo.isPrivate, TLInfo.haveContext, f.iCookies, hostPP, domain_for_cookies);
					var ioService = HTTPUACleaner.ioService;
					
					// Если хост документа не совпадает с хостом вкладки,
					// то это неразрешимая проблема: куда куки девать, т.к. location, кажется, может быть подделан с помощью window.history
					// Хотя я не уверен, что настолько

					let noDocHost = false;
					if (!document.URL || document.URL == 'about:newtab' || document.URL == 'about:blank' || document.URL.indexOf('about:neterror') >= 0 || document.URL == '')
					{
						noDocHost = true;
					}
					
					let docProt = noDocHost ? HTTPUACleaner.getProtocolFromURL(taburl) : document.location.protocol;

					let noCookiesFlag = document.location.host != host;
					let newUri = null;
					try
					{
						if (noDocHost)
							if (f.iCookies > 0)
								newUri = ioService.newURI(docProt + '//' + HTTPUACleaner.getHostByURI(taburl) + '.' + rndStr + '.huac/' + HTTPUACleaner.getPathByURI(taburl), null, null);
							else
							// f.cookie == 2	- если изоляция куков не нужна, а нужна только фильтрация
								newUri = ioService.newURI(taburl, null, null);
						else
							if (f.iCookies > 0)
								newUri = ioService.newURI(document.location.protocol + '//' + document.location.host + '.' + rndStr + '.huac' + document.location.pathname, null, null);
							else
							// f.cookie == 2	- если изоляция куков не нужна, а нужна только фильтрация
								newUri = ioService.newURI(document.URL, null, null);
					}
					catch (e)
					{
						noCookiesFlag = true;

						HTTPUACleaner.logObject(e, true);;
						if (!TLInfo.isPrivate)
						{
							console.error(taburl);
							console.error(document.URL);
							console.error(document.contentType);
						}
					}
				
					let principal = securityManager.getNoAppCodebasePrincipal(newUri);
					let storage = domStorageManager.getLocalStorageForPrincipal(principal, newUri);*/
					return Cu.exportFunction
					(
						function()
						{
							if (aCount++ <= 0)
								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: goName + 'Storage request substitutioned', level: 3});

							return b;/*
							if (noCookiesFlag)
								return null;

							return storage;*/
						}
						, window.wrappedJSObject
					);
				};

				var httpuacleaner_undefined = function()
				{
					if (bCount++ <= 0)
						HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'IndexedDB request blocked', level: 3});

					return undefined;
				};

				Object.defineProperty(window.wrappedJSObject, 	  "localStorage", 	{get: httpuacleaner_localStorageEmpter('local'), set: Cu.exportFunction(httpuacleaner_localStorageEmpterSet,	window.wrappedJSObject), enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "sessionStorage",	{get : httpuacleaner_localStorageEmpter('session'), enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "indexedDB",	{get : Cu.exportFunction(httpuacleaner_undefined,	window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_undefined,	window.wrappedJSObject), enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "mozIndexedDB",	{get : Cu.exportFunction(httpuacleaner_undefined,	window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_undefined,	window.wrappedJSObject), enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "webkitIndexedDB",	{get : Cu.exportFunction(httpuacleaner_undefined,	window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_undefined,	window.wrappedJSObject), enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject, 	  "msIndexedDB",	{get : Cu.exportFunction(httpuacleaner_undefined,	window.wrappedJSObject), set: Cu.exportFunction(httpuacleaner_undefined,	window.wrappedJSObject), enumerable : true, configurable : true});
			}
		};

		var windowNameFunc = function(state)
		{
			if (!document.defaultView)
				return;

			var window = document.defaultView;
			//consoleJSM.console.logp("Window.name disabled for " + host + " / " + document.URL, "Window.name disabled", PRV);
			//HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'window.name', msg: {'filter state': state}});
			logMessages.push({name: 'window.name', state: state});

			var aCount = 0;
			if (state == "raise error")
			{
				var httpuacleaner_wnameEmpter = Cu.exportFunction(function(storage)
				{
					if (aCount++ <= 0)
					HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'window.name get request blocked', level: 3});

					throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to read window.name field. See help for disable this function. For document " + (PRV ? '' : document.location.href));
				}, window.wrappedJSObject);

				var httpuacleaner_httpuacleaner_wnameEmpterSet = Cu.exportFunction(function(storage)
				{
					if (aCount++ <= 0)
					HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'window.name set request blocked', level: 3});

					throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to write window.name field. See help for disable this function. For document " + (PRV ? '' : document.location.href));
				}, window.wrappedJSObject);

				Object.defineProperty(window.wrappedJSObject, "name",	{get : httpuacleaner_wnameEmpter, set: httpuacleaner_httpuacleaner_wnameEmpterSet, enumerable : true, configurable : true});
			}
			else
			if (state == "clean")
			{
				var httpuacleaner_wnameEmpter = Cu.exportFunction(function(storage)
				{
					if (aCount++ <= 0)
					HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'window.name get request blocked', level: 3});

					return "";
				}, window.wrappedJSObject);

				var httpuacleaner_httpuacleaner_wnameEmpterSet = Cu.exportFunction(function(storage)
				{
					if (aCount++ <= 0)
					HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'window.name set request blocked', level: 3});
				}, window.wrappedJSObject);

				Object.defineProperty(window.wrappedJSObject, 	  "name",	{get : httpuacleaner_wnameEmpter, set: httpuacleaner_httpuacleaner_wnameEmpterSet, enumerable : true, configurable : true});
			}
			else
			{
				var httpuacleaner_wunA = function(a)
				{
					try
					{
						// https://yandex.ru/support/disk/troubleshooting/sup-desktop/cant-sync.xml
						// и на других unload вызывается прямо при загрузке страницы (почему-то для фрейма)
						if (window.document.URL && window.document.URL == 'about:blank')
							return;

						if (aCount++ <= 0 && window.name.length > 0)
							HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'window.name cleaned from ' + window.name, level: 3});
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}

					window.name = "";
				};

				window.addEventListener('unload', Cu.exportFunction(httpuacleaner_wunA, window.wrappedJSObject));
			}
		};

		var cookiesFunc = function(state)
		{
			if (!document.defaultView)
				return;
			var window = document.defaultView;

			//consoleJSM.console.logp("document.cookie disabled for " + host + " / " + document.URL, "document.cookie disabled", PRV);
			//HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'document.cookie', msg: {'filter state': state}});
			logMessages.push({name: 'document.cookie', state: state});

			var aCount = 0;
			if (state == "raise error")
			{
				var httpuacleaner_CookieEmpter = Cu.exportFunction
				(
					function()
					{
						if (aCount++ <= 0)
						HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'document.cookie get request blocked', level: 3});

						throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to read document.cookie field. See help for disable this function. For document " + (PRV ? '' : document.location.href));
					},
					window.wrappedJSObject
				);

				var httpuacleaner_httpuacleaner_CookieEmpterSet = Cu.exportFunction
				(
					function()
					{
						if (aCount++ <= 0)
						HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'document.cookie set request blocked', level: 3});

						throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to read document.cookie field. See help for disable this function. For document " + (PRV ? '' : document.location.href));
					},
					window.wrappedJSObject
				);


				Object.defineProperty(document.wrappedJSObject,  "cookie",			{get : httpuacleaner_CookieEmpter, set: httpuacleaner_httpuacleaner_CookieEmpterSet, enumerable : true, configurable : true});

				var metaSetCookie = function()
				{
					var observer = function()
					{
						var metas = document.getElementsByTagName("meta");

						for (var i = 0; i < metas.length; i++)
						{
							var meta = metas[i];
							if (meta['httpEquiv'] && meta['httpEquiv'].toLowerCase() == "set-cookie")
							{
								meta['content'] = '';
							}
						}
					};
					
					document.addEventListener
					(
						"DOMContentLoaded",
						observer
					);
				};

				// http://www.multitran.ru/c/m.exe?a=1&SHL=2
				// <meta HTTP-EQUIV="Set-Cookie" CONTENT="hl=2;EXPIRES=Friday, 01-Jan-2020 23:59:59 GMT;PATH=/">
				// <meta HTTP-EQUIV="Set-Cookie" CONTENT="langs=1 2;EXPIRES=Friday, 01-Jan-2020 23:59:59 GMT;PATH=/">
				metaSetCookie();
			}
			else
			{
				HTTPUACleaner.generateNewiCookieRandomStrIfNotSet(hostPP, host, f.iCookies, domain_for_cookies);

				var rndStr    = HTTPUACleaner.generateCookieRndStr(TLInfo.isPrivate, TLInfo.haveContext, f.iCookies, hostPP, domain_for_cookies);
				var ioService = HTTPUACleaner.ioService;
				
				// Если хост документа не совпадает с хостом вкладки,
				// то это неразрешимая проблема: куда куки девать, т.к. location, кажется, может быть подделан с помощью window.history
				// Хотя я не уверен, что настолько

				var noDocHost = false;
				if (!document.URL || document.URL == 'about:newtab' || document.URL == 'about:blank' || document.URL.indexOf('about:neterror') >= 0 || document.URL == '')
				{
					noDocHost = true;
				}
				
				var docProt = noDocHost ? HTTPUACleaner.getProtocolFromURL(taburl) : document.location.protocol;

				var noCookiesFlag = document.location.hostname != host;
				let newUri = null;
				let oldUri = null;
				let PC  = null;
				let PCC = null;
				try
				{
					if (noDocHost)
						if (f.iCookies > 0)
						{
							newUri = ioService.newURI(docProt + '//' + HTTPUACleaner.getHostByURI(taburl) + '.' + rndStr + '.huac/' + HTTPUACleaner.getPathByURI(taburl), null, null);

							oldUri = ioService.newURI(docProt + '//' + HTTPUACleaner.getHostByURI(taburl) + HTTPUACleaner.getPathByURI(taburl), null, null);
						}
						else
						{
						// f.cookie == 2	- если изоляция куков не нужна, а нужна только фильтрация
							newUri = ioService.newURI(taburl, null, null);
							oldUri = ioService.newURI(taburl, null, null);
						}
					else
						if (f.iCookies > 0)
						{
							newUri = ioService.newURI(document.location.protocol + '//' + document.location.hostname + '.' + rndStr + '.huac' + document.location.pathname, null, null);
							
							oldUri = ioService.newURI(document.location.protocol + '//' + document.location.hostname + document.location.pathname, null, null);
						}
						else
						// f.cookie == 2	- если изоляция куков не нужна, а нужна только фильтрация
						{
							newUri = ioService.newURI(document.URL, null, null);
							oldUri = ioService.newURI(document.URL, null, null);
						}

					if (docProt == 'https:' || docProt == 'wss:')
					{
						PC  = HTTPUACleaner.privateChannelS;
						PCC = HTTPUACleaner.ChannelS;
					}
					else
					{
						PC  = HTTPUACleaner.privateChannelN;
						PCC = HTTPUACleaner.ChannelN;
					}
				}
				catch (e)
				{
					noCookiesFlag = true;

					HTTPUACleaner.logObject(e, true);;
					if (!TLInfo.isPrivate)
					{
						console.error(taburl);
						console.error(document.URL);
						console.error(document.location);
						console.error(document.contentType);
					}
				}

				var httpuacleaner_CookieEmpter = Cu.exportFunction
				(
					function()
					{
						if (aCount++ <= 0)
							if (f.iCookies == 0 && f.cookie != 2)
								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'document.cookie get request blocked', level: 3});
							else
								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'document.cookie get request substituted', level: 3});

						if ((f.iCookies > 0 || f.cookie == 2) && state == 'disabled' && !noCookiesFlag)
						{
							str = HTTPUACleaner.cs.getCookieString(newUri, PC);

							return !str ? '' : str;
						}

						return "";
					},
					window.wrappedJSObject
				);

				var httpuacleaner_httpuacleaner_CookieEmpterSet = Cu.exportFunction
				(
					function(value)
					{
						if (aCount++ <= 0)
						HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'document.cookie set request blocked', level: 3});

						if ((f.iCookies > 0 || f.cookie == 2) && value && state == 'disabled' && !noCookiesFlag)
						{
							var newCk = HTTPUACleaner.changeCookie(value, rndStr, hostPP, f.iCookies, f.cookie);
							if (f.iCookies == 0 && f.cookie == 2)
							{
								return HTTPUACleaner.cs.setCookieString(oldUri, null, newCk, TLInfo.isPrivate ? PC : PCC);
							}
							else
								return HTTPUACleaner.cs.setCookieString(newUri, null, newCk, PC);
						}
						else
							return false;
					},
					window.wrappedJSObject
				);

				Object.defineProperty(document.wrappedJSObject,  "cookie",			{get : httpuacleaner_CookieEmpter, set: httpuacleaner_httpuacleaner_CookieEmpterSet, enumerable : true, configurable : true});

				let metaCookies = [];
				let inObserver  = {state: false};

				var metaSetCookie = function()
				{
					var observer = function()
					{
						if (inObserver.state)
							return;

						inObserver.state = true;
						try
						{

							var toRemove = [];
							var metas = document.getElementsByTagName("meta");

							for (var i = 0; i < metas.length; i++)
							{
								var meta = metas[i];
								if (meta['httpEquiv'] && meta['httpEquiv'].toLowerCase() == "set-cookie")
								{
									metaCookies.push(meta['content']);
									toRemove.push(meta['content']);

									meta.remove();
								}
							}

							let nmCount = 0;
							for (var newCookie of toRemove)
							{
								if (nmCount++ <= 0)
								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'meta http-equiv="set-cookie" request blocked', level: 3});

								if ((f.iCookies > 0 || f.cookie == 2) && newCookie && !noCookiesFlag)
								{
									var newCk = HTTPUACleaner.changeCookie(newCookie, rndStr, hostPP, f.iCookies, f.cookie);
									if (f.iCookies == 0 && f.cookie == 2)
									{
										HTTPUACleaner.cs.setCookieString(oldUri, null, newCk, TLInfo.isPrivate ? PC : PCC);
									}
									else
										HTTPUACleaner.cs.setCookieString(newUri, null, newCk, PC);
								}

								// Удаляем уже установленные через тег meta куки
								var expCk = HTTPUACleaner.changeCookie(newCookie, '', hostPP, 0, 0, true);
								if (TLInfo.isPrivate)
								{
									// Если приватный режим - то PC (при отладке приватный кук может не удалиться, т.к. будет удаляться неприватный)
									HTTPUACleaner.cs.setCookieString(oldUri, null, expCk, PC);
								}
								else
									// Если обычный режим - то PCC, т.к. они не приватные
									HTTPUACleaner.cs.setCookieString(oldUri, null, expCk, PCC);
							}
							
							/*
							var COOKIE = Ci.nsICookie2;
							var mgr = Services.cookies;
							var all = mgr.enumerator;
							
							while ( all.hasMoreElements() )
							{
								var cookie = all.getNext().QueryInterface(Ci.nsICookie2);
								if (cookie.host.endsWith(h))
								{
									myCookies.push(cookie);
								}
							}
							
							for (var idx = myCookies.length - 1; idx > -1; idx--)
							{
								cookie = myCookies[idx];
								mgr.remove(cookie.host, cookie.name, cookie.path, false);
							}*/
						}
						catch (e)
						{
							inObserver.state = false;
						}
						
						inObserver.state = false;
					};

					document.addEventListener
					(
						"DOMContentLoaded",
						observer
					);

					try
					{
						var config = { attributes: true, childList: true, characterData: false, subtree: true };

						// При динамическом создании страницы необходимо ловить все события вставки meta
						var target = document;
						var mobserver = new window.MutationObserver
						(
							function(a)
							{
								observer();
							}
						);

						mobserver.observe(target, config);
						//observer.disconnect();
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}
				};

				// http://www.multitran.ru/c/m.exe?a=1&SHL=2
				// <meta HTTP-EQUIV="Set-Cookie" CONTENT="hl=2;EXPIRES=Friday, 01-Jan-2020 23:59:59 GMT;PATH=/">
				// <meta HTTP-EQUIV="Set-Cookie" CONTENT="langs=1 2;EXPIRES=Friday, 01-Jan-2020 23:59:59 GMT;PATH=/">
				metaSetCookie();
			}
		};
		
		var WebSocketFunc = function(state)
		{
			if (!document.defaultView)
				return;
			var window = document.defaultView;
			//consoleJSM.console.logp("WebSocket disabled for " + host + " / " + document.URL, "WebSocket disabled", PRV);
			//HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'WebSocket'/*, msg: {'filter state': state}*/});
			logMessages.push({name: 'WebSocket', state: state});

			var aCount = 0;
			var httpuacleaner_errorFunction = Cu.exportFunction(function(object)
			{ 
				if (aCount++ <= 0)
				HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'WebSocket request blocked', level: 3});
				
				throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to access to WebSocket object. See help for disable this function. For document " + (PRV ? '' : document.location.href));
			}, window.wrappedJSObject);

			Object.defineProperty(window.wrappedJSObject, 	  "WebSocket", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
		};

		var WebRTCFunc = function(state)
		{
			if (!document.defaultView)
				return;
			var window = document.defaultView;
			logMessages.push({name: 'WebRTC', state: state});
			
			/*		
			https://developer.mozilla.org/en-US/docs/Web/API/WebRTC_API/Introduction
			https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection
			https://developer.mozilla.org/en-US/docs/NavigatorUserMedia.getUserMedia		
			*/
			// https://www.browserleaks.com/webrtc#further-reading
			// window.webkitRTCPeerConnection || window.mozRTCPeerConnection

			var aCount = 0;
			var httpuacleaner_errorFunction = Cu.exportFunction(function(object)
			{ 
				if (aCount++ <= 0)
				HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'WebRTC request blocked', level: 3});
				/*
				throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to access to WebRTC object. See help for disable this function. For document " + (PRV ? '' : document.location.href));*/
			}, window.wrappedJSObject);

			var wndFuncNames = 
			[
				'RTCPeerConnection',
				'RTCIceCandidate',
				'mozRTCIceCandidate',
				'RTCPeerConnectionIceEvent',
				'RTCDataChannel',
				'RTCDataChannelEvent',
				'mozRTCPeerConnection',
				'webkitRTCPeerConnection',
				'RTCSessionDescription',
				'mozRTCSessionDescription',
				'webkitRTCSessionDescription',
				'AudioContext',
				'webkitAudioContext',
				'MediaStream',
				'MediaStreamTrack',
				'MediaDevices',
				'RTCCertificate',
				'RTCRtpReceiver',
				'RTCRtpSender',
				'RTCStatsReport'
			];
			
			var navFuncNames = 
			[
				'getUserMedia',
				'mozGetUserMedia',
				'webkitGetUserMedia'
			];
			
			for (var name of wndFuncNames)
			{
				/*if (!window[name])
					continue;*/

				Object.defineProperty(window.wrappedJSObject, name, 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			}

			for (var name of navFuncNames)
			{
				/*if (!window.navigator[name])
					continue;*/

				Object.defineProperty(window.wrappedJSObject.navigator, name, 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			}
		};

		var AjaxFunc = function(state)
		{
			if (!document.defaultView)
				return;
			var window = document.defaultView;
			//consoleJSM.console.logp("AJAX disabled for " + host + " / " + document.URL, "AJAX disabled", PRV);
			//HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'AJAX'});
			logMessages.push({name: 'AJAX', state: state});

			var aCount = 0;
			var httpuacleaner_errorFunction = Cu.exportFunction(function(object)
			{ 
				if (aCount++ <= 0)
				HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'AJAX request blocked', level: 3});
			
				throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to access to XMLHttpRequest or XMLHttpRequestUpload objects. See help for disable this function. For document " + document.location.href);
			}, window.wrappedJSObject);

			Object.defineProperty(window.wrappedJSObject, 	  "XMLHttpRequest", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

			Object.defineProperty(window.wrappedJSObject, 	  "XMLHttpRequestUpload", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

			Object.defineProperty(window.wrappedJSObject.navigator, "sendBeacon", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
		};

		// https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API
		var FetchFunc = function(state)
		{
			if (!document.defaultView)
				return;
			var window = document.defaultView;
			logMessages.push({name: 'Fetch', state: state});

			var aCount = 0;
			var httpuacleaner_errorFunction = Cu.exportFunction(function(object)
			{ 
				if (aCount++ <= 0)
				HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Fetch request blocked', level: 3});

				return;
			}, window.wrappedJSObject);

			Object.defineProperty(window.wrappedJSObject, 	  "Headers", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

			Object.defineProperty(window.wrappedJSObject, 	  "Response", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

			Object.defineProperty(window.wrappedJSObject.navigator, "Request", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			
			Object.defineProperty(window.wrappedJSObject.navigator, "fetch", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
		};
		
		var PushAPIFunc = function(state)
		{
			if (!document.defaultView)
				return;
			var window = document.defaultView;

			logMessages.push({name: 'PushAPI', state: state});

			var aCount = 0;
			var httpuacleaner_errorFunction = Cu.exportFunction(function(object)
			{ 
				if (aCount++ <= 0)
				HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'PushAPI request blocked', level: 3});
			/*
				throw new Error("HTTP UserAgent Cleaner raise error so how web-script try access to PushAPI. See help for disable this function. For document " + document.location.href);*/
			}, window.wrappedJSObject);

			Object.defineProperty(window.wrappedJSObject, 	  "Notification", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});

			Object.defineProperty(window.wrappedJSObject, 	  "PushManager", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			
			Object.defineProperty(window.wrappedJSObject, 	  "PushSubscription", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
		};
		
		var ServiceWorkerFunc = function(state)
		{
			if (!document.defaultView)
				return;
			var window = document.defaultView;

			logMessages.push({name: 'ServiceWorker', state: state});

			var aCount = 0;
			var httpuacleaner_errorFunction = Cu.exportFunction(function(object)
			{ 
				if (aCount++ <= 0)
				HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'ServiceWorker request blocked', level: 3});
			/*
				throw new Error("HTTP UserAgent Cleaner raise error so how web-script try access to ServiceWorker. See help for disable this function. For document " + document.location.href);*/
			}, window.wrappedJSObject);

			Object.defineProperty(window.wrappedJSObject.navigator, "serviceWorker", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			
			Object.defineProperty(window.wrappedJSObject, 	  "ServiceWorkerRegistration", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			
			// Этого и так нет
			Object.defineProperty(window.wrappedJSObject, 	  "ServiceWorkerGlobalScope", 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
		};

		var ImagesFunc = function(state)
		{
			if (!document.defaultView || (document.contentType && document.contentType.indexOf('image/') == 0))
				return;

			var window = document.defaultView;
			let imgsrc = new Object();

			if (!HTTPUACleaner.imgToPass)
				HTTPUACleaner.imgToPass = [];
			
			let listenerSetted = new Object();
			listenerSetted.yes = false;

			var observer = function(event)
			{
				var srcData = 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7';
				var images = document.getElementsByTagName("img");
				for (var i = 0; i < images.length; i++)
				{
					var img = images[i];
					
					if (img.src.startsWith('resource://') || img.src.startsWith('chrome://'))
					{
						continue;
					}

					if (!img.style)
					{
						if (HTTPUACleaner.debug)
						{
							console.error("Image disable error for " + taburl);
							console.error(img);
						}
						//return;
						continue;
					}

					if (img.id && imgsrc[img.id])
					{
						// src ����������артинки может изменить скрипт
						if (imgsrc[img.id].loaded || img.src == srcData || imgsrc[img.id].toLoad)
							if (img.src == srcData || imgsrc[img.id].src == img.src || img.src == '')
								continue;
							else
							{
								imgsrc[img.id].loaded = false;
								imgsrc[img.id].toLoad = false;
							}
					}
					
					// HTTPUACleaner.loggerB.addToLog(taburl, false, img.src, {type: 'Image'});

					img.style.backgroundRepeat = "no-repeat";
					img.style.backgroundPosition = "center center";
					if (state == 'enabled')
					img.style.backgroundImage = 'url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAASCAYAAABvqT8MAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAAUdEVYdFNvZnR3YXJlAFlhbmRleC5EaXNrTl/4kQAAANJJREFUOE9j3Lhx438GAsBPIhbCMPvIwAKibbKLwXx0cGRqL4PfuTKGTUaLIZpO8TMwQeUYhB7fxsAg8G7uX4imF4vBfLgGfABZE9hJ+ADIWcgArw0mJiYoGATAGkCmPHv2DCwAojdt2gTGIHDmzBkwLSUlBaYZ/kMBKHifPn0KpmEAxkaWg2sAAWTF6AAmB9aArhCfRgaQJLJTsPFhNAiDPc1hbg8OBZBHQTTIg8h8GA0GIF1vZVTApuADIDVwG0gBdNTwTlYVL4YBojIQAjAwAAAoNSWJY1BX6gAAAABJRU5ErkJggg==)';
					else
					//if (state == 'click')
					img.style.backgroundImage = 'url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAASCAYAAABvqT8MAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAAUdEVYdFNvZnR3YXJlAFlhbmRleC5EaXNrTl/4kQAAAMhJREFUOE+9k7ENwjAQRS+IFmjBG9CiiJ4FwgQsxBpMYBe01CCPgSNaxAAm/ydOTEAONDzp9H25+7bPUjKttZcBis22Xky8jKHFfMe8j7kdWDMnXZsemYyamsj6/h7guGhNoDOkiEyDBrPa11FdDyQNeZ6/BKAB7rIs+QFqjGEAay1VKUUV34Dndc5RA2Ed11oDiJv7hBoN/caUUVCMr/IpD4rg0Oq65CtgUCgGjPOghDucp9wlSdXTnvAL/zRcZulo+OoH6hB5AqO3H8MY8tr7AAAAAElFTkSuQmCC)';
					img.style['background-position'] = 'center';
/*
					if (img.id && imgsrc[img.id])
					{
						if (img.src == srcData)
							continue;
						else
						{
							imgsrc[img.id].src = img.src;
							img.src = srcData;
							continue;
						}
					}*/

					var title = img.title;
					
					img.title = '[' + HTTPUACleaner['sdk/l10n'].get("ElementBlocked") + " " + img.src + ']'
						+ (img.title ? " " + img.title : "")
						+ (img.alt && img.alt != img.title ? " " + img.alt : "");
	
					var strW = 0; var strH = 0;
					if (img.outerHTML.toLowerCase().indexOf('width') >= 0)
					{
						var ss = img.outerHTML.toLowerCase().split(/[\s=<>]/);
						var wf = false; var hf = false;
						for (var j = 0; j < ss.length; j++)
						{
							if (ss[j].trim() == 'width' && !wf)
							{
								 var ssk = ss[j + 1];
								 ssk = ssk.split(/[\s'"]/);
								 for (var k = 0; k < ssk.length; k++)
								 {
									 if (ssk[k].length > 0 && ssk[k] != ' ' && ssk[k] != '"' && ssk[k] != "'")
									 {
										 strW = Number(ssk[k]);
										 if (strW == Number.NaN)
											 strW = 0;
										 else
										 {
											 // console.error(strW);
											 wf = true;
											 j++;
											 break;
										 }
									 }
								 }
							}
							else
							if (ss[j].trim() == 'height' && !hf)
							{
								 var ssk = ss[j + 1];
								 ssk = ssk.split(/[\s'"]/);
								 for (var k = 0; k < ssk.length; k++)
								 {
									 if (ssk[k].length > 0 && ssk[k] != ' ' && ssk[k] != '"' && ssk[k] != "'")
									 {
										 strH = Number(ssk[k]);
										 if (strH == Number.NaN)
											 strH = 0;
										 else
										 {
											 // console.error(strH);
											 hf = true;
											 j++;
											 break;
										 }
									 }
								 }
							}

							if (hf && wf)
								break;
						}
					}
					
					if (!img.id)
					{
						img.id = HTTPUACleaner.RandomStr(16);
					}

					imgsrc[img.id] = {src: img.src/*, srcset: img.srcset*/, alt: img.alt, title: title,
										w: img.style.width, h: img.style.height, wt: img.width, ht: img.height,
								   styleW: strW, styleH: strH, id: img.id};

					var w = img.width;
					var h = img.height;
					if (w < 16)
						w = 16;
					if (h < 16)
						h = 16;

					// На случай, если вместо src ис��ол��зо����ан srcset. Если src есть, то берём его
					if (!img.src && img.srcset)
					{
						let newSrc = img.srcset.split(' ')[0];
						imgsrc[img.id].src = newSrc;
						console.error(newSrc);
					}
					
					img.srcset = '';
					img.src    = srcData;
					img.width  = w;
					img.height = h;

					var onClick = function(arg)
						{
							var title         = arg.target.id;

							if (!imgsrc[title] || imgsrc[title].loaded)
								return;

							imgsrc[title].toLoad = true;

							var state		 = HTTPUACleaner.getFunctionState(host, "Images");

							if (state == 'disabled' && !arg.shiftKey || state == 'enabled' && arg.ctrlKey || state == 'click' && !arg.shiftKey)
							{
								// На всякий случай отменяем всё, что возможно
								arg.cancelBubble = true;
								arg.returnValue  = false;
								
								// Отменяем поведение по умолчанию, например, когда картинка стоит в теге a,
								// при клике на неё картинка будет именно подгружаться, а не браузер переходить по ссылке
								if (arg.preventDefault)
									arg.preventDefault();
								
								// Может не быть, если функция вызывается напрямую, без клика пользователя
								if (arg.stopPropagation)
									arg.stopPropagation();

								arg.target.src    = "";
								arg.target.alt    = "";
								arg.target.title  = "";
								//img.style.backgroundImage = "";
								arg.target.style.backgroundImage = "";
								delete arg.target.width;
								delete arg.target.height;
								
								delete arg.target.style.width;
								delete arg.target.style.height;

								window.setTimeout(
									function()
										{
											HTTPUACleaner.imgToPass.push({src: imgsrc[title].src, time: new Date(), founded: false});
											// console.error("imgToPass pushed " + imgsrc[title].src);

											if (imgsrc[title].w && imgsrc[title].w > 16)
											{
												arg.target/*.style*/.width = imgsrc[title].w;
												//console.error('w ' + arg.target/*.style*/.width);
											}
											else
											if (imgsrc[title].styleW && imgsrc[title].styleW > 0)
											{
												arg.target/*.style*/.width = imgsrc[title].styleW;
												//console.error('sw ' + arg.target/*.style*/.width);
											}
											else
											{
												arg.target.style.width = 'auto';
												//console.error('w auto');
											}

											if (imgsrc[title].h && imgsrc[title].h > 16)
											{
												arg.target/*.style*/.height = imgsrc[title].h;
												//console.error('h ' + arg.target/*.style*/.height);
											}
											else
											if (imgsrc[title].styleH && imgsrc[title].styleH > 0)
											{
												arg.target/*.style*/.height = imgsrc[title].styleH;
												//console.error('sh ' + arg.target/*.style*/.height);
											}
											else
											{
												arg.target.style.height = 'auto';
												//console.error('h auto');
											}

											arg.target.src    = imgsrc[title].src;
											arg.target.alt    = imgsrc[title].alt;
											arg.target.title  = imgsrc[title].title;

											arg.target.addEventListener
											(
												'load',
												function(arg)
												{
													if (arg.target.src == imgsrc[title].src)
													{
														imgsrc[title].loaded = true;
													}
												}
											)
										}
										, 0
										);

								return false;
							}
						};
					imgsrc[/*img.title*/img.id].onClick = onClick;

					img.addEventListener('click', onClick);
				}

				var a = document.getElementsByTagName("a");
				for (var i = 0; i < a.length; i++)
				{
					let ae = a[i];

					let founded = [];
					var find = function(node, tagName)
					{
						for (var cn of node.childNodes)
						{
							if (cn.nodeName.toLowerCase() == tagName)
								founded.push(cn.id);	// Реальное src уже заменено, поэтому достаём из imgsrc
						}
						
						for (var cn of node.childNodes)
						{
							find(cn);
						}
					};
					find(ae, 'img');

					// Даже если не нашли img, возможно, мы нашли ссылку на изображение
					//if (founded.length > 0)
					ae.addEventListener
					(
						'mouseup',
						function(arg)
						{
							// arg.target может быть и изображением: событие всплывает вверх
							var foundedFlag = false;
							if (state == 'click' || state == 'enabled' && arg.ctrlKey)
							{
								var link = ae.href; //arg.target.href;

								// Если идёт переход по ссылке, то это может быть переход на изображение
								if (link.indexOf('http:') == 0 || link.indexOf('https:') == 0)
									HTTPUACleaner.imgToPass.push({src: link, time: new Date()});
								else
								if (link.indexOf('//') == 0)
								{
									HTTPUACleaner.imgToPass.push({src: protocol + link, time: new Date()});
								}
								else
									HTTPUACleaner.imgToPass.push({src: protocol + '//' + host + link, time: new Date()});

								for (var imgId of founded)
								{
									link = imgsrc[imgId].src;

									if (!imgsrc[imgId].loaded && !imgsrc[imgId].toLoad)
									{
										var e = document.getElementById(imgId);
										if (e)
										{
											foundedFlag = true;

											if (link.indexOf('http:') ==0 || link.indexOf('https:') == 0)
												HTTPUACleaner.imgToPass.push({src: link, time: new Date()});
											else
												HTTPUACleaner.imgToPass.push({src: protocol + host + link, time: new Date()});

											imgsrc[imgId].onClick({target: e});
										}
									}
								}
							}

							if (foundedFlag)
							{
								//arg.cancelBubble = true;
								//arg.returnValue  = false;
								// arg.stopPropagation();

								arg.preventDefault();
								arg.stopImmediatePropagation();

								return false;
							}
						}
					);
				}

				try
				{
					let number = {value: 0};
					var keydownFunc = function (arg)
					{
						//if (arg.ctrlKey && arg.shiftKey && arg.altKey && arg.keyCode == 73) // если нажато сочетание alt-ctrl-shift-i
						if (/* !arg.ctrlKey && */arg.shiftKey/* && !arg.altKey */&& arg.keyCode == 123) // если нажато сочетание shift-F12
						{
							var alt = arg.altKey;

							if (arg.ctrlKey)
							{
								tab.huacImagesLoad = alt ? 2 : 1;
							}

							let imgArray = [];
							for (var imgId in imgsrc)
							{
								try
								{
									var imgTag = document.getElementById(imgId);
									if (!imgsrc[imgId].loaded && !imgsrc[imgId].toLoad)
										imgArray.push(imgTag);
								}
								catch (e)
								{
									HTTPUACleaner.logObject(e, true);;
								}
							}

							if (imgArray.length <= 0)
							{
								listenerSetted.clicker = false;
								return;
							}

							number.value++;
							let num = number.value;

							listenerSetted.clicker = Date.now();
							var clicker = function(loadFunc)
							{
								// Если всё сделали или если пользователь по второму разу нажал сочетание клавиш, а это обработчик первого сочетания
								if (imgArray.length <= 0 || num < number.value)
								{
									if (imgArray.length <= 0)
									{
										// Сама установит listenerSetted.clicker = false;
										number.func(arg);
									}
									else
									{
									}

									return;
								}

								let imgTag = imgArray[0];
								if (!imgsrc[imgTag.id].loaded)
								{
									imgsrc[imgTag.id].onClick({target: imgTag});
								}

								//imgTag.click()
								imgArray.splice(0, 1);
								
								if (imgsrc[imgTag.id].loaded)
									loadFunc(loadFunc);

								if (!alt)
								{
									let loaded = {completed: false};

									imgTag.addEventListener
									(
										'load',
										function(arg)
										{
											// Не знаю, почему, но load часто вызывается совсем на другой url (похоже, url страницы)
											if (imgsrc[imgTag.id].src == arg.target.src)
											{
												loaded.completed = true;
												loadFunc(loadFunc);
											}
										}
									);

									loaded.timeoutLoad =
										function()
										{
											if (!loaded.completed)
											window.setTimeout
											(
												function()
												{
													if (!loaded.completed)
													{
														if (HTTPUACleaner.httpRequestObserved && HTTPUACleaner.httpRequestObserved.length == 0)
														{
															loaded.completed = 1;
															loadFunc(loadFunc);
														}
														else
															loaded.timeoutLoad();
													}
												},
												1750
											);
										};

									loaded.timeoutLoad();
								}
								else
									window.setTimeout
									(
										function()
										{
											loadFunc(loadFunc);
										},
										//1000
										0
									);
							};

							clicker(clicker);
						}
					};
					number.func = keydownFunc;

					if (state == 'click' && !listenerSetted.yes && document.body/* || state == 'enabled'*/)
					{
						document.body.addEventListener
						(
							'keydown',
							keydownFunc
						);

						listenerSetted.yes = true;
					}


					if (state == 'click' && tab && tab.huacImagesLoad && (!listenerSetted.clicker/* || Date.now() - listenerSetted.clicker > 1*1000*/))
					{
						var arg = {shiftKey: true, keyCode: 123, altKey: tab.huacImagesLoad == 2};

						window.setTimeout
						(
							function()
							{
								keydownFunc(arg);
							},
							0
						);
					}
					else
					{
					}
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
					if (!PRV)
					{
						console.error(document.URL);
						console.error(taburl);
					}
				}
			};

			document.addEventListener
			(
				"DOMContentLoaded",
				observer
			);

			try
			{
				var config = { attributes: true, childList: true, characterData: false, subtree: true };

				var isExecuted = {yes: false};

				// При динамическом создании страницы необходимо ловить все события вставки img
				var target = document;
				var mobserver = new window.MutationObserver
				(
					function()
					{
						if (isExecuted.yes)
							return;

						isExecuted.yes = true;
						try
						{
							window.setTimeout
							(
								function()
								{
									isExecuted.yes = false;
									observer();
								},
								1000
							);
						}
						catch (e)
						{
							isExecuted.yes = false;
							observer();
						}
					}
				);

				mobserver.observe(target, config);
				//observer.disconnect();
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}
		};
		
		var TimeZone = function(state)
		{
			// Фильтр отключён по требованию редакторов Mozilla

		};

		var WebGLFunc = function(state)
		{
			if (!document.defaultView)
				return;
			var window = document.defaultView;
			logMessages.push({name: 'WebGL', state: state});

			var aCount = 0;
			var httpuacleaner_errorFunction = Cu.exportFunction(function(object)
			{ 
				if (aCount++ <= 0)
				HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'WebGL request blocked', level: 3});

			}, window.wrappedJSObject);

			var wndFuncNames = 
			[
				"WebGLShader", "WebGLShaderPrecisionFormat", "WebGLTexture", "WebGLUniformLocation", "WebGLFramebuffer", "WebGLRenderbuffer", "WebGLProgram", "WebGLRenderingContext", "WebGLBuffer", "WebGLActiveInfo"
			];

			for (var name of wndFuncNames)
			{
				Object.defineProperty(window.wrappedJSObject, name, 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			}


			let arrayConstructor = window.wrappedJSObject.Array;
			var getContext = window.wrappedJSObject.HTMLCanvasElement.prototype.getContext;
			var gCnt = 
			function()
			{
				return Cu.exportFunction(
					function(a)
					{
						if (a && a.toLowerCase)
						{
							a = a.toLowerCase();
							if (a == 'webgl' || a == 'experimental-webgl' || a == 'moz-webgl')
								return null;
						}

						var at = new arrayConstructor();
						for (var ar of arguments)
						{
							at.push(ar);
						}

						return getContext.apply(this, at);
					},
					window.wrappedJSObject
				);
			};

			var httpuacleaner_httpuacleaner_CanvasEmpterSet = Cu.exportFunction
			(
				function()
				{
				},
				window.wrappedJSObject
			);

			Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "getContext",	{get : gCnt, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
		};

		// https://audiofingerprint.openwpm.com/
		var AudioCtxFunc = function(state)
		{
			if (!document.defaultView)
				return;

			var window = document.defaultView;
			logMessages.push({name: 'AudioCtx', state: state});

			var aCount = 0;
			var httpuacleaner_errorFunction = Cu.exportFunction(function(object)
			{ 
				if (aCount++ <= 0)
				HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'AudioCtx request blocked', level: 3});

			}, window.wrappedJSObject);

			var wndFuncNames = 
			[
				"OfflineAudioContext", "webkitOfflineAudioContext", "AudioContext", "webkitAudioContext"
			];

			for (var name of wndFuncNames)
			{
				Object.defineProperty(window.wrappedJSObject, name, 	{get : httpuacleaner_errorFunction, set: httpuacleaner_errorFunction, enumerable : true, configurable : true});
			}
		};

		var CanvasFunc = function(state)
		{
			if (!document.defaultView)
				return;

			var window = document.defaultView;

			// Закомментировано для логирования действий по атаке
			/*if (state == 'font')
				return;*/

/*			
				console.error('HTMLCanvasElement');
				console.error(document.defaultView.HTMLCanvasElement);
				console.error(document.defaultView.HTMLElement);
*/
			
			// https://developer.mozilla.org/en-US/docs/Web/API/HTMLCanvasElement
			// WebGLRenderingContext.readPixels
			logMessages.push({name: 'Canvas', state: state});

			var aCount = 0;
			if (state == "raise error" || state == 'clean')
			{
				var httpuacleaner_CanvasEmpter = Cu.exportFunction
				(
					function()
					{
						if (aCount++ <= 0)
						HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Canvas call blocked', level: 3});
						
						if (state == "raise error")
							throw new Error("HTTP UserAgent Cleaner raise error so how web-script try to read canvas.toDataURL or similary field. See help for disable this function. For document " + (PRV ? '' : document.location.href));
					},
					window.wrappedJSObject
				);

				var httpuacleaner_httpuacleaner_CanvasEmpterSet = Cu.exportFunction
				(
					function()
					{
					},
					window.wrappedJSObject
				);


				if (state != 'clean')
				{
					// toDataURL, toBlob, mozGetAsFile, mozFetchAsStream
					Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "toDataURL",			{get : httpuacleaner_CanvasEmpter, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "toBlob",			{get : httpuacleaner_CanvasEmpter, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "mozGetAsFile",			{get : httpuacleaner_CanvasEmpter, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "mozFetchAsStream",			{get : httpuacleaner_CanvasEmpter, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.CanvasRenderingContext2D.prototype,  "getImageData",			{get : httpuacleaner_CanvasEmpter, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
					
					if (window.wrappedJSObject.WebGLRenderingContexts)
					Object.defineProperty(window.wrappedJSObject.WebGLRenderingContexts.prototype,  "readPixels",			{get : httpuacleaner_CanvasEmpter, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
				}
				else
				{
					Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "toDataURL",			{value: undefined, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "toBlob",			{value: undefined, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "mozGetAsFile",			{value: undefined, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "mozFetchAsStream",			{value: undefined, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.CanvasRenderingContext2D.prototype,  "getImageData",			{value: undefined, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.CanvasRenderingContext2D.prototype,  "putImageData",			{value: undefined, enumerable : true, configurable : true});
					
					Object.defineProperty(window.wrappedJSObject.CanvasRenderingContext2D.prototype,  "createImageData",			{value: undefined, enumerable : true, configurable : true});

					if (window.wrappedJSObject.WebGLRenderingContexts)
					Object.defineProperty(window.wrappedJSObject.WebGLRenderingContext.prototype,  "readPixels",			{value: undefined, enumerable : true, configurable : true});
				}
			}
			else
			{
				if (!HTTPUACleaner.HostOptions[hostPP]['Canvas']
				||
				HTTPUACleaner.isHostTimeDied(hostPP, 'Canvas', host) !== false
				)
				{
					HTTPUACleaner.HostOptions[hostPP]['Canvas'] = 
					{
						x0:  HTTPUACleaner.getRandomInt(0,      6),
						y0:  HTTPUACleaner.getRandomInt(0,      6),
						stx: HTTPUACleaner.getRandomInt(1,      6),
						sty: HTTPUACleaner.getRandomInt(1,      6),
						
						a: HTTPUACleaner.getRandomInt(25, 128)/256,
						r: HTTPUACleaner.getRandomInt(16,  255),
						g: HTTPUACleaner.getRandomInt(16,  255),
						b: HTTPUACleaner.getRandomInt(16,  255),

						str: HTTPUACleaner.RandomStr(256),
						quality:  Math.random()/10.0 - 0.05,
						quality2: Math.random()/2.0 + 0.5,
						quality3: Math.random()/2.0 + 0.05
					};

					/*else
					HTTPUACleaner.HostOptions[hostPP]['Canvas'] = 
					{
						x0:  HTTPUACleaner.getRandomInt(0,      6),
						y0:  HTTPUACleaner.getRandomInt(0,      6),
						stx: HTTPUACleaner.getRandomInt(1,      6),
						sty: HTTPUACleaner.getRandomInt(1,      6),
						
						a: HTTPUACleaner.getRandomInt(160, 228)/256,
						r: HTTPUACleaner.getRandomInt(16,  255),
						g: HTTPUACleaner.getRandomInt(16,  255),
						b: HTTPUACleaner.getRandomInt(16,  255)
					};*/
					
					HTTPUACleaner.HostOptions[hostPP]['Canvas'].x1 = HTTPUACleaner.getRandomInt(HTTPUACleaner.HostOptions[hostPP]['Canvas'].x0 + 1, HTTPUACleaner.HostOptions[hostPP]['Canvas'].x0 + 6),
					HTTPUACleaner.HostOptions[hostPP]['Canvas'].y1 = HTTPUACleaner.getRandomInt(HTTPUACleaner.HostOptions[hostPP]['Canvas'].y0 + 1, HTTPUACleaner.HostOptions[hostPP]['Canvas'].y0 + 6),

					HTTPUACleaner.setHostTime(hostPP, 'Canvas', host);
				}

				var cvData = HTTPUACleaner.HostOptions[hostPP]['Canvas'];
				var canvasToSee = HTTPUACleaner.debugOptions.canvasToSee;


				var httpuacleaner_httpuacleaner_CanvasEmpterSet = Cu.exportFunction
				(
					function()
					{
						/*console.error('canvas empter set');
						console.error(arguments);*/
					},
					window.wrappedJSObject
				);

				let putData = {};

				var CanvasRenderingContext2D = window.wrappedJSObject.CanvasRenderingContext2D.prototype;
				let getImageData0 = window.wrappedJSObject.CanvasRenderingContext2D.prototype.getImageData;
				let putImageData0 = window.wrappedJSObject.CanvasRenderingContext2D.prototype.putImageData;
				let getContext = window.wrappedJSObject.HTMLCanvasElement.prototype.getContext;
				var canvasToBadLR = Cu.exportFunction(function(canvas, isCtx)
				{
					var ctx;
					if (isCtx)
						ctx = canvas;
					else
						ctx = getContext.bind(canvas)("2d");

					var w = cvData.x1 - cvData.x0;
					var h = cvData.y1 - cvData.y0;

					ctx.strokeStyle = 'rgba(' + cvData.r.toString() +  ',' + cvData.g.toString() +  ',' + cvData.b.toString() + ',' + cvData.a.toString() + ')';

					try
					{
						var x0 = cvData.x0;
						var y0 = cvData.y0;
						
						var data = getImageData0.bind(ctx)(0, 0, ctx.canvas.width, ctx.canvas.height);
						
						if (putData.data)
						{
							var dt0 = putData.data.data;
							var dt1 = data.data;

							if (dt0.length == dt1.length)
							{
								var diffFound = false;
								for (var i = 0; i < dt0.length; i++)
									if (dt0[i] != dt1[i])
									{
										diffFound = true;
										break;
									}

								if (!diffFound)
								{
									// console.error('CANVAS: no diffs found');
									return;
								}
							}

							/*console.error('CANVAS: diffs founded');
							console.error(dt0);
							console.error(dt1);*/
						}


						var getColor = function(data, i)
						{
							var cl = data[i];
							for (var j = 1; j <= 3; j++)
							{
								cl <<= 8;
								cl += data[i+j];
							}
							
							return cl;
						};

						var getColorDiff = function(cl1, cl2)
						{
							var diff = Math.abs((cl1 & 255) - (cl2 & 255));

							for (var j = 1; j <= 3; j++)
							{
								cl1 >>= 8;
								cl2 >>= 8;
								diff += Math.abs((cl1 & 255) - (cl2 & 255));
							}

							return diff;
						};

						var len = data.data.length;
						var dt  = data.data;
						
						var colorCount = 0;
						var colors = {};
						var clsa   = [{}, {}, {}, {}];
						for (var i = 0; i < len; i += 4)
						{
							var cl = getColor(dt, i);
							var cls = '' + cl;
							if (colors[cls] === undefined)
							{
								colorCount++;
								colors[cls] = cl;

								for (var j = 0; j < 4; j++)
								{
									var dtc = '' + (dt[i+j] >> 5);
									if (!clsa[j][dtc])
										clsa[j][dtc] = [];

									clsa[j][dtc].push(cl);
								}
							}
						}
/*console.error(colorCount);
console.error(data.data.length);
var dtn1 = window.Date.now();*/

						var colorsDiff = {};
						const cdMax = 40;
						for (var acl in colors)
						{
							var a = [];
							colorsDiff[acl] = a;

							var tmp = {};
							var acla = [];
							var aclt = Number(acl);
							for (var j = 3; j >= 0; j--)
							{
								acla[j] = (aclt & 255) >> 5;
								aclt >>= 8;
							}
							aclt = Number(acl);

							for (var j = 0; j < 4; j++)
							{
								var tmp2 = [clsa[j][acla[j]], clsa[j][acla[j]-1], clsa[j][acla[j]+1]];

								for (var k = 0; k < tmp2.length; k++)
								{
									if (tmp2[k])
									for (var cl of tmp2[k])
									{
										if (!tmp[cl])
											tmp[cl] = 1;
										else
											tmp[cl]++;
									}
								}
							}

							for (var cl2 in tmp)
							{
								if (tmp[cl2] < 4)
									continue;

								if (cl2 != acl)
								{
									var cd = getColorDiff(aclt, Number(cl2));
									if (cd <= cdMax)
										a.push(  [cd, cl2]  );
								}
							}

							a.sort
							(
								function(a, b)
								{
									return a[0] - b[0];
								}
							);
						}

						var isLowRandom = state == 'low random';
						var str = cvData.str;
						var incrementColor = function(cl, i, low)
						{
							var newCl;
							if (cl & 255 >= 255 - 5 - 1)
							{
								if ((isLowRandom || low) && colorCount > 256)
									newCl = cl - 1;
								else
								{
									var d = (str.charCodeAt(i % str.length) & 14) >> 1;
									if (d == 0)
										newCl = cl - 1;
									else
									if ((isLowRandom || low) || colorCount > 256)
										newCl = cl - 2;
									else
										newCl = cl - 1 - d;
								}
							}
							else
							{
								if ((isLowRandom || low) && colorCount > 256)
									newCl = cl + 1;
								else
								{
									var d = (str.charCodeAt(i % str.length) & 14) >> 1;
									if (d == 0)
										newCl = cl + 1;
									else
									if ((isLowRandom || low) || colorCount > 256)
										newCl = cl + 2;
									else
										newCl = cl + 1 + d;
								}
							}

							return newCl;
						};

						var findNewColor = function(cl, i)
						{
							var newCl;
							var minDiff = Number.MAX_SAFE_INTEGER;

							var acl = colorsDiff['' + cl];
							for (var aacl of acl)
							{
								if ((str.charCodeAt((aacl[0] + i) % str.length) & 1) > 0)
								{
									minDiff = aacl[0];
									newCl = aacl[1];
									break;
								}
							}

							if (minDiff > cdMax)
							{
								newCl = incrementColor(cl, minDiff + i, false);
							}

							return newCl;
						};

						var w2  = (ctx.canvas.width << 2);
						var a   = isLowRandom ? [0, 0, 0] : [0, 0];
						for (var i = 0; i < len; i += 4)
						{
							var differenceFound = 0;
							a[0] = i + 4;
							a[1] = i + w2;
							if (isLowRandom)
								a[2] = i + w2 + 1;

							var ccl = getColor(dt, i);
							for (var v of a)
							{
								if (v >= 0 && v < len)
								{
									if (ccl != getColor(dt, v))
									{
										differenceFound++;
									}
								}
							}

							if ((isLowRandom && differenceFound >= 3) || (!isLowRandom && differenceFound > 0))
							{
								var newa = findNewColor(ccl, i);

								dt[i+3] = newa & 255;
								newa >>= 8;
								dt[i+2] = newa & 255;
								newa >>= 8;
								dt[i+1] = newa & 255;
								newa >>= 8;
								dt[i+0] = newa & 255;
							}
						}
/*var dtn2 = window.Date.now();
console.error((dtn2-dtn1)/1000);*/
						putImageData0.bind(ctx)(data, 0, 0);

						putData.data = getImageData0.bind(ctx)(0, 0, ctx.canvas.width, ctx.canvas.height);
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}
				},
				window.wrappedJSObject
				);

				var quality  = cvData.quality;
				var quality2 = cvData.quality2;
				var quality3 = cvData.quality3;

				// let createImageBitmap0 = window.wrappedJSObject.createImageBitmap;
				let toBlob = window.wrappedJSObject.HTMLCanvasElement.prototype.toBlob;
				let toDataURL = window.wrappedJSObject.HTMLCanvasElement.prototype.toDataURL;
				// let drawImage = window.wrappedJSObject.CanvasRenderingContext2D.prototype.drawImage;
				var canvasToBadR = Cu.exportFunction(function(canvas, isCtx)
				{
					var ctx;
					if (isCtx)
						ctx = canvas;
					else
						ctx = getContext.bind(canvas)("2d");

					var w = cvData.x1 - cvData.x0;
					var h = cvData.y1 - cvData.y0;

					ctx.strokeStyle = 'rgba(' + cvData.r.toString() +  ',' + cvData.g.toString() +  ',' + cvData.b.toString() + ',' + cvData.a.toString() + ')';

					try
					{
						var x0 = cvData.x0;
						var y0 = cvData.y0;
						
						var data = getImageData0.bind(ctx)(0, 0, ctx.canvas.width, ctx.canvas.height);
						
						if (putData.data)
						{
							var dt0 = putData.data.data;
							var dt1 = data.data;

							if (dt0.length == dt1.length)
							{
								var diffFound = false;
								for (var i = 0; i < dt0.length; i++)
									if (dt0[i] != dt1[i])
									{
										diffFound = true;
										break;
									}

								if (!diffFound)
								{
									return;
								}
							}
						}

						var len = data.data.length;
						var dt  = data.data;
						var ct  = dt; //Array.from(dt);
						var str = cvData.str;
						var cnt = 0;
						var it  = 0;
						var d   = 255;
						var st  = 0;
						var a1a = [1, 0, 1];
						var a2a = [0, 1, 1];
						do
						{
							for (var i = 0; i < len; i += 4)
							{
								it++;
								for (var j = 0; j < 3; j++)
								{
									var a1 = a1a[j] << 2;
									var a2 = a2a[j] << 2;
									var k = i + a1 + a2 * ctx.canvas.width;
									var found = false;
									for (var c = 0; c < 4; c++)
									{
										if (i + c < len && k + c < len)
										if (ct[i + c] != ct[k + c])
										{
											dt[i + c] = ct[k + c];
											dt[k + c] = ct[i + c];
											if (!found)
												cnt++;

											found = true;
										}
									}
								}

								i += (str.charCodeAt((i + cnt + d) % str.length) & d) << 2;
							}

							if (st & 1 > 0)
							{
								d++;
								d >>= 1;
								d--;
							}

							st++;

							if (d < 3)
								d = 3;
						}
						while (len / cnt > (64 << 2) && it < len);

						putImageData0.bind(ctx)(data, 0, 0);

						putData.data = getImageData0.bind(ctx)(0, 0, ctx.canvas.width, ctx.canvas.height);
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}
				},
				window.wrappedJSObject
				);
				
				var canvasToBad = Cu.exportFunction(function(canvas, isCtx)
				{
					var ctx;
					if (isCtx)
						ctx = canvas;
					else
						ctx = getContext.bind(canvas)("2d");

					var w = cvData.x1 - cvData.x0;
					var h = cvData.y1 - cvData.y0;

					ctx.strokeStyle = 'rgba(' + cvData.r.toString() +  ',' + cvData.g.toString() +  ',' + cvData.b.toString() + ',' + cvData.a.toString() + ')';
					
					var x0 = cvData.x0;
					var y0 = cvData.y0;
					for (var i = x0; i + w < ctx.canvas.width; i += w + cvData.stx)
					for (var j = y0; j + h < ctx.canvas.height; j += h + cvData.sty)
					{
						ctx.strokeRect(i, j, w, h);
						ctx.strokeRect(i + (w >> 1), j + (h >> 1), 1, 1);
					}
				},
				window.wrappedJSObject
				);

				if (state == 'random')
					canvasToBad = canvasToBadR;
				else
				if (state == 'low random')
					canvasToBad = canvasToBadLR;
				else
				if (state == 'font')
					canvasToBad = Cu.exportFunction
					(
						function(canvas, isCtx)
						{
							var ctx;
							if (isCtx)
								ctx = canvas;
							else
								ctx = getContext.bind(canvas)("2d");

							putData.data = getImageData0.bind(ctx)(0, 0, ctx.canvas.width, ctx.canvas.height);
						},
						window.wrappedJSObject
					);

				var toQualityNumber = Cu.exportFunction
				(
					// type, quality
					function(a2, a3)
					{
						var a3 = Number(a3);

						if (Number.isNaN(a3))
						{
							a3 = undefined;
							// https://developer.mozilla.org/en-US/docs/Web/API/HTMLCanvasElement/toDataURL
							if (a2)
							if (a2.toLowerCase() == 'image/jpeg' || a2.toLowerCase() == 'image/webp')
							{
								a3 = quality2;
								if (a3 < 0.1)
									a3 = 0.1;
								else
								if (a3 >= 1.0)
									a3 = 1.0;
							}
						}
						else
						{
							a3 *= 1.0 + quality;
							if (a3 >= 1.0)
								a3 = a3 - quality*2;
						}

						return a3;
					},
					window.wrappedJSObject
				)

				var httpuacleaner_CanvasToDataURL = 
				function()
				{
					return Cu.exportFunction
					(
						function(a1, a2)
						{
							try
							{
								canvasToBad(this);
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
							}

							if (aCount++ <= 5)
							{
								var obj = {type: 'Canvas call emulated', level: 3, msg: {'function': 'toDataURL'}};
								if (canvasToSee)
									obj.msg['__canvasdata'] = putData.data;
								else
									obj.msg['__canvasdata'] = 
										{
											width:  putData.data ? putData.data.width  : undefined,
											height: putData.data ? putData.data.height : undefined
										};

								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, obj);
							}


							return toDataURL.bind(this)(a1, toQualityNumber(a1, a2));
						},
						window.wrappedJSObject
					);
				};

				var httpuacleaner_CanvastoBlob = 
				function()
				{
					return Cu.exportFunction
					(
						function(a1, a2, a3)
						{
							try
							{
								canvasToBad(this);
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
							}

							if (aCount++ <= 5)
							{
								var obj = {type: 'Canvas call emulated', level: 3, msg: {'function': 'toBlob'}};
								if (canvasToSee)
									obj.msg['__canvasdata'] = putData.data;
								else
									obj.msg['__canvasdata'] = 
										{
											width:  putData.data ? putData.data.width  : undefined,
											height: putData.data ? putData.data.height : undefined
										};

								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, obj);
							}

							return toBlob.bind(this)(a1, a2, toQualityNumber(a2, a3));
						},
						window.wrappedJSObject
					);
				};

				let mozGetAsFile = window.wrappedJSObject.HTMLCanvasElement.prototype.mozGetAsFile;
				var httpuacleaner_CanvasmozGetAsFile = 
				function()
				{
					return;
					/*return Cu.exportFunction
					(
						function(a1, a2)
						{
							try
							{
								canvasToBad(this);
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
							}

							if (aCount++ <= 5)
							HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Canvas call emulated', level: 3, msg: {'function': 'mozGetAsFile', '__canvasdata': putData.data}});

							return mozGetAsFile.bind(this)(a1, a2);
						},
						window.wrappedJSObject
					);*/
				};

				let mozFetchAsStream = window.wrappedJSObject.HTMLCanvasElement.prototype.mozFetchAsStream;
				var httpuacleaner_CanvasmozFetchAsStream = 
				function()
				{
					return;
					/*
					return Cu.exportFunction
					(
						function(a1, a2)
						{
							try
							{
								canvasToBad(this);
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
							}

							if (aCount++ <= 5)
							HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Canvas call emulated', level: 3, msg: {'function': 'mozFetchAsStream', '__canvasdata': putData.data}});

							return mozFetchAsStream.bind(this)(a1, a2); //this.mozFetchAsStream(a1, a2);
						},
						window.wrappedJSObject
					);*/
				};
				
				let getImageData = window.wrappedJSObject.CanvasRenderingContext2D.prototype.getImageData;
				var httpuacleaner_CanvasgetImageData = 
				function()
				{
					return Cu.exportFunction
					(
						function(sx, sy, sw, sh)
						{
							try
							{
								canvasToBad(this, true);
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
							}

							if (aCount++ <= 5)
							{
								var obj = {type: 'Canvas call emulated', level: 3, msg: {'function': 'getImageData'}};
								if (canvasToSee)
									obj.msg['__canvasdata'] = putData.data;
								else
									obj.msg['__canvasdata'] = 
										{
											width:  putData.data ? putData.data.width  : undefined,
											height: putData.data ? putData.data.height : undefined
										};

								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, obj);
							}

							var result = getImageData.bind(this)(sx, sy, sw, sh);//this.getImageData(sx, sy, sw, sh);

							return result;
						},
						window.wrappedJSObject
					);
				};
				
				let putImageData = window.wrappedJSObject.CanvasRenderingContext2D.prototype.putImageData;
				var httpuacleaner_CanvasputImageData = 
				function()
				{
					return Cu.exportFunction
					(
						function(imagedata, dx, dy, dirtyX, dirtyY, dirtyWidth, dirtyHeight)
						{
							try
							{
								var result = putImageData.bind(this)(imagedata, dx, dy, dirtyX, dirtyY, dirtyWidth, dirtyHeight);

								var data = getImageData0.bind(ctx)(0, 0, ctx.canvas.width, ctx.canvas.height);
								putData.data = data;
							}
							catch (e)
							{
								HTTPUACleaner.logObject(e, true);;
							}

							if (aCount++ <= 5)
							{
								var obj = {type: 'Canvas call emulated', level: 3, msg: {'function': 'putImageData'}};
								if (canvasToSee)
									obj.msg['__canvasdata'] = putData.data;
								else
									obj.msg['__canvasdata'] = 
										{
											width:  putData.data ? putData.data.width  : undefined,
											height: putData.data ? putData.data.height : undefined
										};

								HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, obj);
							}

							return result;
						},
						window.wrappedJSObject
					);
				};

				var httpuacleaner_CanvasgetreadPixels = null;
				if (window.wrappedJSObject.WebGLRenderingContext)
				{
					let readPixels = window.wrappedJSObject.WebGLRenderingContext.prototype.readPixels;
					httpuacleaner_CanvasgetreadPixels = 
					function()
					{
						return Cu.exportFunction
						(
							function(x, y, width, height, format, type, pixels)
							{
								try
								{
									canvasToBad(this, true);
								}
								catch (e)
								{
									HTTPUACleaner.logObject(e, true);;
								}

								if (aCount++ <= 5)
								{
									var obj = {type: 'Canvas call emulated', level: 3, msg: {'function': 'readPixels'}};
									if (canvasToSee)
										obj.msg['__canvasdata'] = putData.data;
									else
										obj.msg['__canvasdata'] = 
										{
											width:  putData.data ? putData.data.width  : undefined,
											height: putData.data ? putData.data.height : undefined
										};

									HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, obj);
								}

								var result = readPixels.bind(this)(x, y, width, height, format, type, pixels);

								return result;
							},
							window.wrappedJSObject
						);
					};
				}

				Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "toDataURL",			{get : httpuacleaner_CanvasToDataURL, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
				
				Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "toBlob",			{get : httpuacleaner_CanvastoBlob, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
				
				Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "mozGetAsFile",			{get : httpuacleaner_CanvasmozGetAsFile, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : false, configurable : true});

				Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "mozFetchAsStream",			{get : httpuacleaner_CanvasmozFetchAsStream, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
				
				Object.defineProperty(window.wrappedJSObject.HTMLCanvasElement.prototype,  "captureStream",			{get : httpuacleaner_httpuacleaner_CanvasEmpterSet, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject.CanvasRenderingContext2D.prototype,  "getImageData",			{get : httpuacleaner_CanvasgetImageData, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
				
				Object.defineProperty(window.wrappedJSObject.CanvasRenderingContext2D.prototype,  "putImageData",			{get : httpuacleaner_CanvasputImageData, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});

				if (httpuacleaner_CanvasgetreadPixels)
				Object.defineProperty(window.wrappedJSObject.WebGLRenderingContext.prototype,  "readPixels",			{get : httpuacleaner_CanvasgetreadPixels, set: httpuacleaner_httpuacleaner_CanvasEmpterSet, enumerable : true, configurable : true});
			}
		};

		var FontsFunc = function(state, canvasState)
		{
			if (!document.defaultView)
				return;

			var safetyFonts = 
				['Arial', 'Arial Black', 'Courier New', 'Georgia', 'Impact', 'Times New Roman', 'Verdana', 'Monospace', 'Fantasy', 'Cursive', 'Serif', 'Sans-Serif', 'inherit'];
			var safetyFonts2 = 
				['Arial', 'Arial Black', 'Courier New', 'Georgia', 'Impact', 'Times New Roman', 'Verdana', 'Monospace', 'Fantasy', 'Cursive'];

			var window = document.defaultView;

			var ffGet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.CSS2Properties.prototype, 'fontFamily').get, window.wrappedJSObject);
			var ffSet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.CSS2Properties.prototype, 'fontFamily').set, window.wrappedJSObject);
			
			var fontGet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.CSS2Properties.prototype, 'font').get, window.wrappedJSObject);
			var fontSet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.CSS2Properties.prototype, 'font').set, window.wrappedJSObject);
/*			
			var setPropertyGet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.CSS2Properties.prototype, 'setProperty').get, window.wrappedJSObject);
			var setPropertySet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.CSS2Properties.prototype, 'setProperty').set, window.wrappedJSObject);
*/
			var fonts = [];

			if (!HTTPUACleaner.HostOptions[hostPP]['Fonts'] || HTTPUACleaner.isHostTimeDied(hostPP, 'Fonts', host) !== false)
			{
				HTTPUACleaner.HostOptions[hostPP]['Fonts'] = {fontsAllowed: [], fontsBlocked: [], rndA: HTTPUACleaner.getRandomInt(0, safetyFonts.length - 1), rndB: HTTPUACleaner.getRandomInt(0, safetyFonts2.length), rndF: HTTPUACleaner.getRandomInt(10, 90), rndS: HTTPUACleaner.getRandomInt(-2, +2)};
				HTTPUACleaner.setHostTime(hostPP, 'Fonts', host);
			}

			var fontsAllowed = HTTPUACleaner.HostOptions[hostPP]['Fonts'].fontsAllowed;
			var fontsBlocked = HTTPUACleaner.HostOptions[hostPP]['Fonts'].fontsBlocked;
			var rndA         = HTTPUACleaner.HostOptions[hostPP]['Fonts'].rndA;
			var rndB         = HTTPUACleaner.HostOptions[hostPP]['Fonts'].rndB;
			var rndF         = HTTPUACleaner.HostOptions[hostPP]['Fonts'].rndF;
			var rndS         = HTTPUACleaner.HostOptions[hostPP]['Fonts'].rndS;
			if (rndS == 0)
				rndS = -3;	// от -3 до +1


			var getFontName = function(fontDeclaration, onlyName)
			{
				fontDeclaration = fontDeclaration.trim();
				var fd = fontDeclaration.split(',');
				fontDeclaration = fd[0];
				var a1 = fontDeclaration.indexOf("'");
				var a2 = fontDeclaration.indexOf('"');
				if (a1 >= 0)
				{
					fontDeclaration = fontDeclaration.substr(a1 + 1);
					
					var a = fontDeclaration.indexOf("'");
					if (a < 0)
					{
						console.error("not found ' in font name");
						return 'serif';
					}
					
					return fontDeclaration.substr(0, a);
				}
				else
				if (a2 >= 0)
				{
					fontDeclaration = fontDeclaration.substr(a2 + 1);
					var a = fontDeclaration.indexOf('"');
					if (a < 0)
					{
						console.error('not found " in font name');
						return 'serif';
					}
					
					return fontDeclaration.substr(0, a);
				}
				else
				{
					if (onlyName)
						return fontDeclaration;

					var fd = fontDeclaration.split(' ');
					return fd[fd.length - 1];
				}
			};

			var f4096 = 0;
			var addFonts = function(font, alwaysRandom)
			{
				if (fonts.indexOf(arguments[0]) < 0)
				{
					// Это для того, чтобы вредоносный скрипт не мог бы переполнить память
					if (fonts.length < 4*1024)
						fonts.push(arguments[0]);
					else
					{
						if (f4096++ <= 0)
							HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'FONT > 4096 attack detected', level: 3});

						return safetyFonts[rndA]; // 'Courier New'; //false;
					}

					if (fonts.length == 64)
					{
						HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Font attack detected', level: 3});
					}
				}

				if (state == 'random' || alwaysRandom)
				{
					var fb = fontsBlocked.indexOf(arguments[0]);
					if (fb >= 0)
					{
						//if (fontsAllowed.length == 0)
							return safetyFonts[rndA]; // 'Courier New';

						//return fontsAllowed[fb % fontsAllowed.length];
					}

					if (fontsAllowed.indexOf(arguments[0]) < 0)
					{
						if (  HTTPUACleaner.getRandomInt(0, 100) >= rndF )
						{
							fontsBlocked.push(arguments[0]);

							//if (fontsAllowed.length == 0)
								return safetyFonts[rndA]; // 'Courier New';

							//return fontsAllowed[(fontsBlocked.length - 1) % fontsAllowed.length];
						}
						else
						{
							fontsAllowed.push(arguments[0]);
							return arguments[0];
						}
					}
					else
						return arguments[0];
				}

				return arguments[0];
			};

			// Это нужно для canvas=random и canvas=font
			for (var sf of safetyFonts2)
				addFonts(sf);


			var httpuacleaner_FontFamily = 
			function()
			{
				return ffGet.apply(this, arguments);
			};
			
			var httpuacleaner_FontFamilySet = Cu.exportFunction
			(
				function()
				{
					var fontName = getFontName(arguments[0], true);

					if (safetyFonts.indexOf(fontName) >= 0)
						return ffSet.apply(this, arguments);
					else
					{
						var fontAllowed = addFonts(fontName);
						if (fontAllowed === false)
							return;

						if (state != 'random')
						{
							var timeout = 1000;
							if (fonts.length > 16)
								timeout = 5000;
							if (fonts.length > 32)
								timeout = 10000;

							var t = this;
							window.setTimeout
							(
								function()
								{
									ffSet.apply(t, arguments);
								},
								timeout
							);
						}
						else
							return ffSet.apply(this, [fontAllowed]);
					}
				},
				window.wrappedJSObject
			);
			
			var httpuacleaner_Font = 
			function()
			{
				return fontGet.apply(this, arguments);
			};

			var httpuacleaner_FontSet = Cu.exportFunction
			(
				function()
				{
					// Атака может быть такая, что первый шрифт никогда не будет найден, поэтому остальные вообще отбрасываем, иначе совсем замучиться можно ещё учитывать, что перебор может б��ть по второму шрифту
					var aSplit = arguments[0].split(',');
					var FA = aSplit[0];
					var fontName = getFontName(arguments[0]);

					if (safetyFonts.indexOf(fontName) >= 0)
						return fontSet.apply(this, [FA]);
					else
					{
						var fontAllowed = addFonts(fontName);
						if (fontAllowed === false)
							return;

						if (fontName != fontAllowed)
							FA = arguments[0].replace(fontName, fontAllowed);

						if (state != 'random')
						{
							var timeout = 1000;
							if (fonts.length > 16)
								timeout = 5000;
							if (fonts.length > 32)
								timeout = 10000;

							var t = this;
							window.setTimeout
							(
								function()
								{
									fontSet.apply(t, [FA]);
								},
								timeout
							);
						}
						else
							return fontSet.apply(this, [FA]);
					}
				},
				window.wrappedJSObject
			);

			var setPropertyFunc = function(a, b, c)
			{
				if (a == 'font-family' || a == 'fontFamily' || a == 'font')
				{
					if (a == 'font')
						httpuacleaner_FontSet.call(this, b);
					else
						httpuacleaner_FontFamilySet.call(this, b);
				}
				else
					window.CSS2Properties.prototype.setProperty.apply(this, [a, b, c]);
			};
			
			var ctxFontGet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.CanvasRenderingContext2D.prototype, 'font').get, window.wrappedJSObject);
			var ctxFontSet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.CanvasRenderingContext2D.prototype, 'font').set, window.wrappedJSObject);
			
			var httpuacleaner_CtxFontSet = Cu.exportFunction
			(
				function()
				{
					var fontName = getFontName(arguments[0]);

					var aSplit = arguments[0].split(',');
					var FA = aSplit[0];

					if ((canvasState != 'random' && canvasState != 'font') && safetyFonts2.indexOf(fontName) >= 0)
						return ctxFontSet.apply(this, arguments);
					else
					{
						var size = arguments[0].split(' ');
						if (size.length < 2)
							return;
						
						size = size[0];

						// 14px
						// [ "14px", "14", "px" ]
						var rr = /^([0-9]+)(.*)$/.exec(size);
						if (!rr || rr.length < 2)
						{
							return;
						}

						var snum = Number(rr[1]) + rndS;
						var sstr = rr.length > 2 ? rr[2] : '';

						size = snum + sstr;

						var fontAllowed = addFonts(fontName, true);
						if (fontAllowed === false)
							return;

						if (fontName != fontAllowed)
						{
							//FA = arguments[0].replace(fontName, fontAllowed);
							FA = size + ' ' + safetyFonts2[rndB]; //fontAllowed;
						}
						else
						{
							// FA = arguments[0].replace(fontName, fontName + ',' + safetyFonts[rndA]);
							FA = size + " '" + fontName + "', '" + safetyFonts2[rndB] + "'";
						}

						return ctxFontSet.apply(this, [FA]);
					}
				},
				window.wrappedJSObject
			);
			
			var httpuacleaner_CtxFont = Cu.exportFunction(function()
			{
				return ctxFontGet.apply(this, arguments);
			}, window.wrappedJSObject);
			
			var httpuacleaner_setAttribute = Cu.exportFunction(function(name, value)
			{
				if (name == 'style' && value && value.replace)
				{
					value = value.replace(/font[ \t\r\n]*:/ig, 'HUACreplacedF:');	// прост�� делаем декларацию шрифтов недействительной
					return window.HTMLElement.prototype.setAttribute.apply(this, [name, value]);
				}
				
				return window.HTMLElement.prototype.setAttribute.apply(this, arguments);
			}, window.wrappedJSObject);
			
			
			var styleGet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.HTMLElement.prototype, 'style').get, window.wrappedJSObject);
			var styleSet = Cu.exportFunction(Object.getOwnPropertyDescriptor(window.wrappedJSObject.HTMLElement.prototype, 'style').set, window.wrappedJSObject);

			var httpuacleaner_StyleGet = Cu.exportFunction
			(
				function()
				{
					return styleGet.apply(this, arguments);
				}, window.wrappedJSObject
			);
			
			var httpuacleaner_StyleSet = Cu.exportFunction(function(object)
			{
				var font = null;
				var FF   = null;
				if (object.font)
				{
					font = object.font;
					delete object.font;
				}

				if (object['fontFamily'])
				{
					FF = object['fontFamily'];
					delete object['fontFamily'];
				}
				else
				if (object['font-family'])
				{
					FF = object['font-family'];
					delete object['font-family'];
				}

				styleSet.apply(this, arguments);

				if (font || FF)
				{
					if (font)
						httpuacleaner_FontSet.apply(styleGet.apply(this, []), [font]);
					if (FF)
						httpuacleaner_FontFamilySet.apply(styleGet.apply(this, []), [FF]);
				}
			}, window.wrappedJSObject);

			if (state != 'disabled')
			{
				Object.defineProperty(window.wrappedJSObject.CSS2Properties.prototype,  "fontFamily",	{get: httpuacleaner_FontFamily, set: httpuacleaner_FontFamilySet, enumerable : true, configurable : true});
				Object.defineProperty(window.wrappedJSObject.CSS2Properties.prototype,  "font-family",	{get: httpuacleaner_FontFamily, set: httpuacleaner_FontFamilySet, enumerable : true, configurable : true});
				Object.defineProperty(window.wrappedJSObject.CSS2Properties.prototype,  "font",	{get: httpuacleaner_Font, set: httpuacleaner_FontSet, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject.CSS2Properties.prototype,  "setProperty",	{value: setPropertyFunc, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject.HTMLElement.prototype,  "setAttribute",			{value: httpuacleaner_setAttribute, enumerable : true, configurable : true});

				Object.defineProperty(window.wrappedJSObject.HTMLElement.prototype,  "style",			{get: httpuacleaner_StyleGet, set: httpuacleaner_StyleSet, enumerable : true, configurable : true});
			}

			Object.defineProperty(window.wrappedJSObject.CanvasRenderingContext2D.prototype,  "font",			{get: httpuacleaner_CtxFont, set: httpuacleaner_CtxFontSet, enumerable : true, configurable : true});
		};
		
		
		var PasswordFunc = function(state)
		{
			if (!document.defaultView)
				return;

			var window = document.defaultView;
			//var pwdsrc = new Object();
			var founded = {};
			var fndA    = {};
			var observerStarted = {value: false};

			var observer = function(event)
			{
				if (observerStarted.value === true)
					return;

				observerStarted.value = true;

				// restore не работает
				var disableForm = function(el, founded, restore)
				{
					if (state != 'click' && !restore)
					{
						if (el.form)
						{
							el.form.action = '';
							el.form.method = '';
						}
					}

					var els = el.form ? el.form.elements : [el];

					var aNames = ['disabled', 'readOnly', 'title', 'placeholder'];
					for (var i = 0; i < els.length; i++)
					{
						var elSubmit = els[i];
						var idn      = '' + elSubmit.id + '|' + elSubmit.name;
						if (!elSubmit.type /*|| elSubmit.type.toLowerCase() != 'submit' elSubmit.type.toLowerCase() == 'password'*/)
						{
							continue;
						}

						/*if (restore === true)
						{
							for (var a of aNames)
								elSubmit[a] = fndA[idn][a];

							elSubmit.style['background-color'] = fndA[idn]['background-color'];
							elSubmit.style['color']            = fndA[idn]['color'];
						}
						else*/
						if (founded === false || !elSubmit.disabled || !el.readOnly)
						{/*
							if (!fndA[idn])
							{
								var obj = {};

								for (var a of aNames)
									obj[a] = elSubmit[a];

								obj['background-color'] = window.getComputedStyle(el)['background-color'];
								obj['color']            = window.getComputedStyle(el)['color'];
								fndA[idn] = obj;
							}*/

							elSubmit.disabled = true;
							elSubmit.readOnly = true;

							elSubmit.title       = HTTPUACleaner['sdk/l10n'].get('Blocked by HUAC');
							if (elSubmit.type != 'submit')
							{
								elSubmit.placeholder = HTTPUACleaner['sdk/l10n'].get('Blocked by HUAC');
							}
							/*else
								elSubmit.value	     = HTTPUACleaner['sdk/l10n'].get('Blocked by HUAC');*/

							var rgb = window.getComputedStyle(el)['background-color'].toLowerCase();

							if (rgb.indexOf('rgba(0') >= 0 || rgb == 'transparent')
							{
								el.style['background-color'] = '#777777';
								el.style['color'] = '#000000';
							}
							
							if (founded === false)
							{
								el.addEventListener(/*'click',*/ 'mousedown',
									function(arg)
									{
										//if (state != 'click' || !arg.ctrlKey)
										disableForm(arg.target, true);

										//arg.target.disabled = true;

										window.setTimeout
										(
											function()
											{
											//	if (state != 'click' || !arg.ctrlKey)
												disableForm(arg.target, true);
												/*else
												{
													disableForm(arg.target, true, true);
												}*/
											}
											, 0
										);

										return false;
									}
								);

								el.addEventListener
								('keydown',
									function(arg)
									{
										disableForm(arg.target, true);

										window.setTimeout(
												function()
												{
													disableForm(arg.target, true);
												}
												, 0
												);

										return false;
									}
								);
							}
						}
					}
				};
				
				try
				{
					var inputs = document.getElementsByTagName("input");

					for (var i = 0; i < inputs.length; i++)
					{
						var el = inputs[i];

						if (!el.type || el.type.toLowerCase() != 'password')
						{
							continue;
						}

						if (founded[el.id + '|' + el.name] !== true)
						{
							disableForm(el, false);

							HTTPUACleaner.loggerB.addToLog(taburl, false, taburl, {type: 'Password', level: 3, msg: {id: el.id, name: el.name}});
							// console.error('Password founded');
							founded[el.id + '|' + el.name] = true;
						}
						
						if (!el.disabled/* || !el.readOnly*/)
						{
							disableForm(el, true);
						}
					}
				}
				catch (e)
				{
					observerStarted.value = false;
					throw e;
				}

				observerStarted.value = false;
			};

			document.addEventListener
			(
				"DOMContentLoaded",
				function()
				{
					try
					{
						window.setTimeout
						(
							function()
							{
								observer();
							},
							0
						);
					}
					catch (e)
					{
						// HTTPUACleaner.logObject(e, true);;
						observer();
					}
				}
			);

			try
			{
				var config = { attributes: true, childList: true, characterData: false, subtree: true };

				var isExecuted = {yes: false};

				// При динамическом создании стран��цы необходимо л��ви��ь все ��об��ти�� вставки input
				var target = document;
				var obsFunc = function(mutations)
					{
						if (isExecuted.yes)
							return;

						isExecuted.yes = true;

						var func = function()
							{
								isExecuted.yes = false;
								observer();
							};

						try
						{
							window.setTimeout
							(
								func,
								500
							);
						}
						catch (e)
						{
							// HTTPUACleaner.logObject(e, true);;
							func();
						}
					};
					
				var mobserver = new window.MutationObserver(obsFunc);

				mobserver.observe(target, config);
				//observer.disconnect();
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		};

		var state		 = HTTPUACleaner.getFunctionState(host, "Plugins");
		if (state != "disabled" && host)
		{
			try
			{
				PluginsFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "Locale");
		if (state != "disabled" && host)
		{
			try
			{
				LocaleFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "DNT");
		if (state != "disabled" && host)
		{
			try
			{
				DNTFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "Screen");
		if (state != "disabled" && host)
		{
			try
			{
				ScreenFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "Storage");
		if (state != "disabled" || f.iCookies > 0 || f.cookie == 2)
		{
			try
			{
				storageFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "wname");
		if (state != "disabled" && host)
		{
			try
			{
				windowNameFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "dCookies");
		if (state != "disabled" || f.iCookies > 0 || f.cookie == 2)
		{
			try
			{
				cookiesFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "WebRTC");
		if (state != "disabled" && host)
		{
			try
			{
				WebRTCFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "WebSocket");
		if (state != "disabled" && host)
		{
			try
			{
				WebSocketFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "AJAX");
		if (state != "disabled" && host)
		{
			try
			{
				AjaxFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "Fetch");
		if (state != "disabled" && host)
		{
			try
			{
				FetchFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "PushAPI");
		if (state != "disabled" && host)
		{
			try
			{
				PushAPIFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "ServiceWorker");
		if (state != "disabled" && host)
		{
			try
			{
				ServiceWorkerFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "Images");
		if (state != "disabled" && host)
		{
			try
			{
				ImagesFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "TimeZone");
		if (state != "disabled" && host)
		{
			try
			{
				TimeZone(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "WebGL");
		if (state != "disabled" && host)
		{
			try
			{
				WebGLFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "AudioCtx");
		if (state != "disabled" && host)
		{
			try
			{
				AudioCtxFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var state		 = HTTPUACleaner.getFunctionState(host, "Canvas");
		if (state != "disabled" && host)
		{
			try
			{
				CanvasFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		var canvasState  = state;
		var state		 = HTTPUACleaner.getFunctionState(host, "Fonts");
		if ((state != "disabled" || canvasState != "disabled") && host)
		{
			try
			{
				FontsFunc(state, canvasState);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "Password");

		if (state != "disabled" && host)
		{
			try
			{
				PasswordFunc(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
		}

		if (logMessages.length > 0)
		{
			var resultMsg = {};
			for (var a of logMessages)
			{
				resultMsg[a.name] = a.state ? a.state : '';
				if (a.action)
					resultMsg[a.name] += ': ' + a.action;
			}

			if (resultMsg)
			{
				resultMsg['tab'] = taburl;
				HTTPUACleaner.loggerB.addToLog(taburl, false, rUrl, {type: 'document', level: 1, msg: resultMsg});
			}
		}
	},
	// ----- end onDocumentCreated


	// дублировано в popupmenu.js
	getDomainByHost: require("./getURL").getDomainByHost,

	getHostByURI: require("./getURL").getHostByURI,
	getPathByURI: require("./getURL").getPathByURI,

	lastHost: "",

	getHostNameCount: 0,

	getHostName: function(httpChannel, subject, cached, context, object)
	{
		var httpHost = HTTPUACleaner.getHostByURI(httpChannel.URI.spec);//String(httpChannel.originalURI.host);
		var document = null;

		if (!object)
			object = {};

		object.taburl = '';

		if (context)
		{
			var homeFound = false;
			try
			{
				var urls = HTTPUACleaner.urls;
				var browser = urls.getBrowserForContext(context);


				if (!browser)
					throw Error('!browser');

				// result плохой в первом запросе, если с одной странице в той же вкладке перейти на другую
				var taburl = urls.fromBrowser(browser);
				var result = HTTPUACleaner.getHostByURI(taburl);
				object.taburl = taburl;
				
				// В content-policy иногда приходят запросы, которые генерированы window.history
				// Они будут запомнены в cpurl, но не в url, поэтому url вкладки не изменится для дополнения
				if (object.onHttpRequestOpened === true && browser.huac && browser.huac.cpurl == httpChannel.URI.spec)
				{
					browser.huac.url = browser.huac.cpurl;
					browser.huac.cpurl = null;
				}
				else
				{
					/*if (browser.huac && browser.huac.cpurl == httpChannel.URI.spec)
						console.error('!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!');*/
				}

				object.huac = browser.huac;

				// При самой первой подгрузке нет content-policy
				if (HTTPUACleaner.getHostNameCount < 1)
				{
					HTTPUACleaner.getHostNameCount++;
					
					// Проверяем, что подгрузка идёт именно от старта браузера, а не приложения
					if (HTTPUACleaner.tabsListener.lastUrls.length == 0)
					{
						if (object.onHttpRequestOpened === true && browser.userTypedClear == 2 && (taburl && taburl.indexOf('about:neterror') == 0 || taburl == "about:newtab" || taburl == "about:blank" || taburl == ''))
						{
							var obj     = {};
							obj.url     = httpChannel.URI.spec;
							obj.taburl  = taburl;
							obj.time	= Date.now();
							obj.work    = 0;
							HTTPUACleaner.tabsListener.lastUrls.push(obj);
						}
					}
				}
				/*
				if ((taburl.indexOf('about:neterror') >= 0 || taburl == "about:newtab" || taburl == "about:blank" || taburl == '') && url.indexOf('about:') != 0)
				{
					obj.taburl  = httpChannel.URI.spec;
				}
		*/
		
				var Location = null;
				try
				{
					Location = httpChannel.getResponseHeader("Location");
				}
				catch (e)
				{
				}
		
				var foundedObj = null;
				try
				{
					var npSpec = urls.getURIWithoutScheme(httpChannel.URI.spec);
					var setHomeFound = function(obj, obji)
					{
						foundedObj = obj;

						homeFound  = httpHost;
						/*if (obj.ltu)
						{
							console.error('obj.ltu');
							console.error(obj);
							console.error(obj.ltu);
							console.error(httpChannel.URI.spec);
						}*/

						// Если не response или есть очередное перенаправление
						/*if (!object.filled || Location)
						{
							obj.ltu      = obj.taburl;
							obj.Location = Location;
						}*/
						/*else
							delete obj.ltu;*/

						obj.taburl = httpChannel.URI.spec;

						object.HTTPUACleaner_URI = httpChannel.URI.spec;
						object.taburl = httpChannel.URI.spec;

						obj.time = Date.now();

						// Раньше было obj.work++, но
						// onHttpRequestOpened вызывается лишний раз, когда идёт не http перенаправление на https
						// Что это за чудо творит FireFox я не понял
						if (object && object.filled)
						{
							if (!Location)
								obj.work = 2;
						}
						
						// Это тоже относится к переходу на https
						if (obj.url != httpChannel.URI.spec && object.onHttpRequestOpened)
						{
							HTTPUACleaner.tabsListener.lastUrls[obji].url = httpChannel.URI.spec;
						}

						object.homeFound = obj;
/*
						if (obj.work >= 2)
						{
							HTTPUACleaner.tabsListener.lastUrls.splice(obji, 1);
							// console.error(HTTPUACleaner.tabsListener.lastUrls);
						}*/
					};

					for (var obji in HTTPUACleaner.tabsListener.lastUrls)
					{
						var obj = HTTPUACleaner.tabsListener.lastUrls[obji];
						/*
						console.error('search');
						console.error(httpChannel.URI.spec);
						//console.error(obj.url);
						console.error(taburl);
						console.error(obj.taburl);
						console.error('' + obj.filled + ' ' + object.onHttpRequestOpened);
						console.error(obj.ltu);
						console.error(urls.getURIWithoutScheme(obj.url) == npSpec);
						*/

						// Почему-то иногда происходит не http-перенаправление с http на https
						if (urls.getURIWithoutScheme(obj.url) == npSpec)
						{
							if
							(
								// ltu - это ��дрес вкладки / старый документ, который в нём открыт
								// Он устанавливается в content-policy
								(/*obj.ltu && */obj.ltu == taburl/* && (object.filled || obj.Location)*/)
								//||
								//(obj.taburl == taburl && /*object.onHttpRequestOpened*/ (!object.filled && !obj.Location))
								||
								((obj.taburl === '' || obj.taburl == taburl) && (taburl && taburl.indexOf('about:neterror') >= 0 || taburl == 'about:blank' || taburl == 'about:newtab' || taburl == ''))
							)
							{
								setHomeFound(obj, obji);
								break;
							}
						}
					}

					var ct = Date.now();
					for (var obji in HTTPUACleaner.tabsListener.lastUrls)
					{
						var obj = HTTPUACleaner.tabsListener.lastUrls[obji];
						if (obj.work >= 2 || ct - obj.time > 1 * 60 * 1000)
						{
							HTTPUACleaner.tabsListener.lastUrls.splice(obji, 1);
						}
					}
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
				}
/*
				if (homeFound || result == "about:blank" || result == "about:newtab" || true)
				{
					console.error('------ Http UserAgent Cleaner ------------------------------------------------');
					console.error('homeFound: ' + homeFound);
					console.error('result: ' + result);
					//console.error('userTypedClear: ' + tab.linkedBrowser.userTypedClear);
					console.error('cached ' + cached);
					console.error('url: ' + httpChannel.URI.spec);
					console.error('object: ');
					console.error(object);
					console.error('lastUrls:');
					console.error(HTTPUACleaner.tabsListener.lastUrls);
				}
*/

				if (homeFound !== false)	// в том числе, когда переходит на домашнюю страницу
				{
					var oldResult = result;
					result     = homeFound;
					
					// console.error('homeFound ' + homeFound);
				}
				else
					foundedObj = {};

					

					// Если пользователь ввёл какое-то значение (или нажал кнопку "домашняя страница"),
					// значит url, полученный из tab некорректен
					// if (homeFound)
					if (browser && browser.huac && browser.huac.url == httpChannel.URI.spec)
					{
						// Если редирект, то меняем домен на домен редиректа
						try
						{
							var newLocation = httpChannel.getResponseHeader("Location");

							if ((httpChannel.responseStatus >= 301 && httpChannel.responseStatus <= 304) || newLocation)
							{
								var addToLog = function(redirectTo)
								{
									try
									{
										HTTPUACleaner.loggerB.addToLog(homeFound ? httpChannel.URI.spec : taburl, redirectTo, httpChannel.URI.spec, {type: 'Redirect allowed', level: 0, msg: {'From': httpChannel.URI.spec, 'To': redirectTo}});
									}
									catch (e)
									{
										if (HTTPUACleaner.debug)
										{
											console.error("HTTPUACleaner.loggerB.addToLog has raised exception");
											HTTPUACleaner.logObject(e, true);;
										}
									}
								};

								var setBrowserHuac = function(newLocation)
								{
									if (browser && browser.huac && browser.huac.url == httpChannel.URI.spec)
										browser.huac.url = newLocation;

									// console.error('http redirect. huac.url: ' + newLocation);
								};

								if (newLocation.indexOf('http:') == 0 || newLocation.indexOf('https:') == 0 || newLocation.indexOf('wss:') == 0)
								{
									object.redirectTo = newLocation;

									foundedObj.url = newLocation;
									
									addToLog(newLocation);
									setBrowserHuac(newLocation);
								}
								else
								{
									var NL;
									if (newLocation.indexOf('/') == 0)
									{
										var hs = httpChannel.URI.spec.split(':')[0] + '://' + httpHost;
										NL = hs + newLocation;
									}
									else
									{
										if (httpChannel.URI.spec[httpChannel.URI.spec.length - 1] == '/')
											NL = httpChannel.URI.spec + newLocation;
										else
											NL = httpChannel.URI.spec + '/' + newLocation;
									}

									object.redirectTo = NL;

									foundedObj.url = NL;
									addToLog(NL);
									/*if (browser && browser.huac && browser.huac.url == httpChannel.URI.spec)
										browser.huac.url = NL;*/
									setBrowserHuac(NL);
								}
							}
						}
						catch (e)
						{
							// HTTPUACleaner.logObject(e, true);;
						}

					}/*
					else
					{
						if (HTTPUACleaner.debug)
							consoleJSM.console.logp
							(
								"HTTPUACleaner: Tab host SWITCHED to " + httpHost + " from " + result,
								"HTTPUACleaner: Tab host SWITCHED",
								true
							);

						result = httpHost;
					}*/

				// result.PRV = PRV;
				result.noTab = false;

				if (result.indexOf("about:") != 0 && result != "")
				{
					return result;
				}
				else
				{
					object.taburl = httpChannel.URI.spec;
					result        = httpHost;
					// result.PRV    = PRV;
					result.noTab  = true;

					return result;
				}
			}
			catch(e)
			{
				/*if (HTTPUACleaner.debug)
				consoleJSM.console.logp("HUAC. No tab for " + httpChannel.URI.spec, "HUAC. No tab", PRV);*/

				object.taburl = '';
				var result    = httpHost;
				// result.PRV    = PRV;
				result.noTab  = true;
				return result;
			}
		}

		/*if (HTTPUACleaner.debug)
			consoleJSM.console.logp("HUAC. No context for " + httpChannel.URI.spec, "HUAC. No tab", PRV);*/

		object.taburl  = '';
		// httpHost.PRV   = PRV;
		httpHost.noTab = true;
		return httpHost;
	},
	
	// ----- end getHostName
	
	isOCSPRequest: function(httpChannel)
	{
		if (httpChannel.requestMethod != "POST")
			return false;
		
		var found = false;
		var len   = false;

		// tools.ietf.org/html/rfc2616
		var toFind = ['Content-Type', 'Content-Length'];
		var visitor = function() {};
		visitor.prototype.visitHeader = function(header, value)
		{
			if (header == 'Content-Type')
			{
				if (value == 'application/ocsp-request')
					found = true;
			}
			else
			if (header == 'Content-Length')
			{
				len = value;
			}
		};

		httpChannel.visitRequestHeaders(new visitor());
		
		if (!found)
			return false;
		/*if (len > 200)
			return false;*/

		return true;
	},
	
	isOCSPResponse: function(httpChannel, obj, code)
	{
		var found = false;
		var len   = false;
		obj.HPKP  = false;

		// tools.ietf.org/html/rfc2616
		var toFind = ['Content-Type', 'Content-Length'];
		var visitor = function() {};
		visitor.prototype.visitHeader = function(header, value)
		{
			if (header == 'Content-Type')
			{
				if (value == 'application/ocsp-response')
					found = true;
			}
			else
			if (header == 'Content-Length')
			{
				len = value;
			}
			else
			// https://tools.ietf.org/html/rfc7469
			if (header == 'Public-Key-Pins')
			{
				// Если вдруг нескол��ко одинаковых заголовков
				obj['Public-Key-Pins'] = null;

				// max-age=1296000; pin-sha256="r/mIkG3eEpVdm+u/ko/cwxzOMo1bk4TyHIlByibiA5E="; pin-sha256="WoiWRyIOVNa9ihaBciRSC7XHjliYS9VwUGOIud4PB18=";
				// max-age=600; includeSubDomains; pin-sha256="WoiWRyIOVNa9ihaBciRSC7XHjliYS9VwUGOIud4PB18="; pin-sha256="5C8kvU039KouVrl52D0eZSGf4Onjo4Khs8tmyTlV3nU="; pin-sha256="5C8kvU039KouVrl52D0eZSGf4Onjo4Khs8tmyTlV3nU="; pin-sha256="lCppFqbkrlJ3EcVFAkeip0+44VaoJUymbnOaEUk7tEU="; pin-sha256="TUDnr0MEoJ3of7+YliBMBVFB4/gJsv5zO7IxD9+YoWI="; pin-sha256="x4QzPSC810K5/cMjb05Qm4k3Bw5zBn4lTdO/nEW/Td4=";
				// report-uri
				if (value.indexOf('pin-sha') >= 0)
				{
					var ageIndex = value.indexOf('max-age=');
					if (ageIndex < 0)
					{
						console.error('HUAC information: HPKP header for ' + httpChannel.URI.spec + ' is incorrect (max-age): ' + value);
						
						obj.HPKP = false;
						return;
					}

					var splitted = value.split(';');
					for (var spl of splitted)
					{
						if (spl.trim().length <= 0)
							continue;

						var sp = spl.split('=');
						if (sp.length <= 0)
						{
							console.error('HUAC information: HPKP header for ' + httpChannel.URI.spec + ' is incorrect (";"): ' + value);
							
							obj.HPKP = false;
							return;
						}

						var sp0 = sp[0].trim();
						if (sp.length == 1)
						{
							if (sp0 != 'includeSubDomains')
							{
								console.error('HUAC information: HPKP header for ' + httpChannel.URI.spec + ' is incorrect ("!="): ' + value);
								
								obj.HPKP = false;
								return;
							}

							continue;
						}

						// Знак равно может стоять в кавычках в base64
						if (sp.length >= 2)
						{
							if (sp0 == 'pin-sha256')
							{
								continue;
							}

							if (sp0 == 'report-uri')
							{
								continue;
							}

							if (sp0 == 'max-age')
							{
								continue;
							}
						}

						console.error('HUAC information: HPKP header for ' + httpChannel.URI.spec + ' is incorrect (0): ' + value);
						console.error('Incorrect HPKP directive: "' + sp0 + '"');

						obj.HPKP = false;
						return;
					}

					obj['Public-Key-Pins'] = value;

					var val = value.substr(ageIndex + 'max-age='.length);
					var ageIndex = val.indexOf(';');
					if (ageIndex < -1)
						ageIndex = val.length;

					val = Number(val.substr(0, ageIndex));
					if (Number.isNaN(val) || val <= 1)
						obj.HPKP = 0;

					obj['max-age'] = val;
					obj.HPKP = val / (3600*24);
				}
				else
				{
					obj.HPKP = false;
					return;
				}
			}
		};

		httpChannel.visitResponseHeaders(new visitor());

		// Если 304, то сначала вызывается examine, а потом merge (или наоборот???)
		// code == "ImLYE2mChLYJvHcV9EvuPYwV1B9" указывает на то, что заголовки также получены закешированными
		// В версии FireFox 49/50 они не кешируются, однако, т.к. нельзя отличить, были ли они ранее, мы считаем, как будто их не было
		try
		{
			// len === false && httpChannel.contentLength <= 0 работает неверно в SPDY
			if (/*len === false && httpChannel.contentLength <= 0 || */len === false && httpChannel.responseStatus >= 304 && httpChannel.responseStatus <= 304/* || code == "ImLYE2mChLYJvHcV9EvuPYwV1B9"*/)
			{
				obj.haveContent = false;
			}
			else
			{
				obj.haveContent = true;
			}
		}
		catch (e)
		{
			obj.haveContent = true;
		}

		// Не перемещать выше, т.к. ещё анализируется наличие HPKP
		if (httpChannel.requestMethod != "POST")
			return false;

		if (!found)
			return false;
		/*if (len > 200)
			return false;*/

		return true;
	},

	isOnlyHttps: function(host, RequestHost)
	{
		// Осто��ожно! Без скобки будет возвращать undefined
		return (
			HTTPUACleaner.getFunctionState(host, "OnlyHttps") != 'disabled'
			||
			// nonGlobal == true, т.е. глобальная настройка фильтра будет учитываться,
			// только если не установлено другой настройки для этого конкретного домена
			HTTPUACleaner.getFunctionState(RequestHost, "OnlyHttps", 'disabled') != 'disabled'
			);

			/*
			Смысл nonGlobal в том, чтобы при переходе по ссылке или загрузке во фрейм при глобальном OnlyHTTPS,
			не-https ссылки с доменов, разрешённых для http, блокировались только в том случае, если другой домен однозначно
			разрешён только на https
			То есть, ЖЖ, например, должен подгружать со своих поддоменов всё. Но если захочет подгрузить с другого домена,
			который однозначн�� указан как HTTPS, то б����дет блок
			В то же время, например, подгрузка http://mc.yandex.ru/metrika/watch.js должна быть заблокирована,
			если на yandex.ru установлен HTTPS фильтр
			При этом, скажем, переход по ссылке на Яндекс со страницы https://meduza.io/feature/2015/06/15/zabvenie-prava-na-poisk
			должен быть только https
			*/
	},
	
	// Отладочная функция
	getAllRequestHeaders: function(httpChannel)
	{
		var headers = [];
		var visitor = function() {};
		visitor.prototype.visitHeader = function(header, value)
		{
			headers.push(header + ': ' + value);
		};

		httpChannel.visitRequestHeaders(new visitor());
		
		return headers;
	},
	
	// Отладочная фу��кция
	getAllResponseHeaders: function(httpChannel)
	{
		var headers = [];
		var visitor = function() {};
		visitor.prototype.visitHeader = function(header, value)
		{
			headers.push(header + ': ' + value);
		};

		httpChannel.visitResponseHeaders(new visitor());
		
		return headers;
	},

	// Для режимов I1, I2, I3 - инициализация идёт после инициализации объекта
	// CookieRandomStr = [[], [], []]
	// HTTPUACleaner.setCookieRandomStr()
	setCookieRandomStr: function(regenerateChannels)
	{
		if (!HTTPUACleaner.privateChannelS || !HTTPUACleaner.privateChannelN || regenerateChannels)
		{
			var ios = HTTPUACleaner.ioService;
			
			// https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/nsIIOService#newChannel%28%29
			// aContentPolicyType == 1 поставл��н ��екорректно, т.к. реально канал открывается для любых типов
			HTTPUACleaner.ChannelS = ios.newChannel2('https://vlR67XPsVbC4.huac', null, null, null, null, null, null, 0);
			HTTPUACleaner.ChannelN = ios.newChannel2('http://vlR67XPsVbC4.huac', null, null, null, null, null, null, 0);

			HTTPUACleaner.privateChannelS = ios.newChannel2('https://vlR67XPsVbC4.huac', null, null, null, null, null, null, 0);
			HTTPUACleaner.privateChannelN = ios.newChannel2('http://vlR67XPsVbC4.huac', null, null, null, null, null, null, 0);

			var intPVCN = HTTPUACleaner.privateChannelN.QueryInterface(Ci.nsIPrivateBrowsingChannel);
			var intPVCS = HTTPUACleaner.privateChannelS.QueryInterface(Ci.nsIPrivateBrowsingChannel);

			if (!HTTPUACleaner['sdk/preferences/service'].get(HTTPUACleaner_Prefix + 'debug.nonPrivateCookieIsolation', false))
			{
				intPVCN.setPrivate(true);
				intPVCS.setPrivate(true);
			}
		}

		HTTPUACleaner.CookieRandomStr = 
		[
			[HTTPUACleaner.RandomStr(24) + '-i1', HTTPUACleaner.RandomStr(24) + '-i1', HTTPUACleaner.RandomStr(24) + '-i1', HTTPUACleaner.RandomStr(24) + '-i1'],
			[HTTPUACleaner.RandomStr(24) + '-i2', HTTPUACleaner.RandomStr(24) + '-i2', HTTPUACleaner.RandomStr(24) + '-i2', HTTPUACleaner.RandomStr(24) + '-i2'],
			[HTTPUACleaner.RandomStr(24) + '-i3', HTTPUACleaner.RandomStr(24) + '-i3', HTTPUACleaner.RandomStr(24) + '-i3', HTTPUACleaner.RandomStr(24) + '-i3']
		];
		
		HTTPUACleaner.CookieRandomStrDomain = {};
	},

	generateCookieRndStr: function(isPrivate, haveContext, regime, hostPP, domain_for_cookies)
	{
		var pos = 0;
		if (isPrivate)
			pos += 1;
		if (haveContext)
			pos += 2;

		var rndStr = HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies].Rnd[pos]; //HTTPUACleaner.HostOptions[hostPP]['iCookies'].random[pos];
		if (regime >= 1000)
			rndStr = HTTPUACleaner.CookieRandomStr[regime-1000][pos];
		else
		if (regime == 2)
			rndStr = HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies].Iso[pos]; //HTTPUACleaner.HostOptions[hostPP]['iCookies'].Iso[pos];
		else
		if (regime == 3)
			rndStr = HTTPUACleaner.HostOptions[hostPP]['iCookies'].IsoH[pos];

		if (!rndStr)
		{
			console.error('HUAC ERROR: in generateCookieRndStr !rndStr: ' + isPrivate + '/' + haveContext + '/' + regime);
			return HTTPUACleaner.RandomStr(24);
		}

		return rndStr;
	},
	
	generateNewiCookieRandomStrIfNotSet: function(hostPP, host, regime, domain_for_cookies)
	{
		if (
			!HTTPUACleaner.HostOptions[hostPP]['iCookies']
			||
			(
				regime == 1
				&&
				HTTPUACleaner.isHostTimeDied(hostPP, 'iCookies', host) !== false
			)
			/*||
			(
				Math.abs(HTTPUACleaner.HostOptions[hostPP]['iCookies'].regime - regime) > 900
			)*/
		)
		{
			HTTPUACleaner.generateNewiCookieRandomStr(hostPP, host, regime, domain_for_cookies);
		}
	},

	// hostPP - хост с префиксом приватного режима (используется ":")
	generateNewiCookieRandomStr: function(hostPP, host, regime, domain_for_cookies)
	{
		if (!HTTPUACleaner.HostOptions[hostPP]['iCookies'])
			HTTPUACleaner.HostOptions[hostPP]['iCookies'] = {};

		/*
		HTTPUACleaner.HostOptions[hostPP]['iCookies'].random = 
				[HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24)];

		if (!HTTPUACleaner.HostOptions[hostPP]['iCookies'].Iso)
			HTTPUACleaner.HostOptions[hostPP]['iCookies'].Iso = 
					[HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24)];*/

		if (!HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies])
			HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies] = {};

		if (!HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies].Iso)
		{
			HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies].Iso = 
					[HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24)];
			HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies].Rnd = 
					[HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24)];
		}

		if (!HTTPUACleaner.HostOptions[hostPP]['iCookies'].IsoH)
			HTTPUACleaner.HostOptions[hostPP]['iCookies'].IsoH = 
					[HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24), HTTPUACleaner.RandomStr(24)];

		HTTPUACleaner.setHostTime(hostPP, 'iCookies', host);
	},
	
	LogHeaders: {},
	LogHeadersCnt: 0,

	LogRequestLoadFlags: function(httpChannel, msg, lh)
	{
		if (httpChannel.loadFlags)
		{
			loadFlags = '';
			
			if (!HTTPUACleaner.LogRequestLoadFlags.consts)
			{
				HTTPUACleaner.LogRequestLoadFlags.consts = {};
				for (var loadConst in Ci.nsIRequest)
				{
					if (loadConst.indexOf('_') < 0)
						continue;

					HTTPUACleaner.LogRequestLoadFlags.consts[loadConst] = Ci.nsIRequest[loadConst];
				}
			}

			// https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/NsIRequest
			for (var loadConst in HTTPUACleaner.LogRequestLoadFlags.consts)
			{
				var a = HTTPUACleaner.LogRequestLoadFlags.consts[loadConst];
				if (a && !a.indexOf && !Number.isNaN(Number(a)) && (a & 65535 > 0))
				if ((httpChannel.loadFlags & 65535) & a > 0)
				{
					loadFlags += loadConst + ' ';
				}
			}

			if (loadFlags.length > 0)
				msg[(lh.barCnt++) + '------'] = {name: 'loadFlags', val: '' + httpChannel.loadFlags.toString(16).toUpperCase() + 'h: ' + loadFlags};
		}
	},
	
	LogRequestHeaders: function(subject, httpChannel, taburl, host, obj, context, modify, isPrivateChannel)
	{
		var lh;
		for (var hn in HTTPUACleaner.LogHeaders)
		{
			lh = HTTPUACleaner.LogHeaders[hn];
			if (lh.subject == subject)
				break;

			lh = null;
		}

		var created = false;
		if (!lh)
		{
			lh = {};
			lh.hcnt = 0;
			lh.barCnt = 0;
			lh.headers = {pre: {}, r: {}, resp: {}}
			lh.serviceData = {url: (httpChannel.URI ? httpChannel.URI.spec : ''), method: httpChannel.requestMethod};
			lh.serviceData.status = 0;
			created = true;
			lh.serviceData.stopped = 0;
			lh.serviceData.headers = lh.headers;
			lh.serviceData.start = Date.now();
			lh.serviceData.channelId = httpChannel.channelId; // "{2f1d48e1-404f-4d98-8497-1094f911f2ef}"

			if (modify && Date.now() - HTTPUACleaner.startupTime > 15000)
			{
				HTTPUACleaner.logMessage('HUAC ERROR: LogRequestHeaders not found for request ' + httpChannel.URI.spec, true);
				lh.info = {name: 'info (skipped)', val: 'error occured or "get data" setting is 0', bgcolor: '#FF00033'};
			}
			else
				lh.info = {name: 'info (skipped)', val: 'skipped because HUAC not initialized or error occured or "get data" setting is 0', bgcolor: '#FF00033'};
		}
		else
		if (modify)
			return lh;

		HTTPUACleaner.LogHeaders['' + HTTPUACleaner.LogHeadersCnt] = lh;
		lh.cnt = '' + HTTPUACleaner.LogHeadersCnt++;
		lh.subject = subject;

		var msg;
		if (lh.msg)
			msg = lh.msg
		else
		{
			msg = {'tab': taburl};
			lh.msg = msg;
			msg.received = 0;
			lh .received = 0;
			
			var uri = httpChannel.URI ? httpChannel.URI.spec : '';
			/*try
			{
				uri = decodeURI(uri);
			}
			catch (e)
			{}
*/
			try
			{
				msg[(lh.barCnt++) + '------'] = {name: 'url', val: httpChannel.requestMethod + ' ' + uri, noDecode: true, bgcolor: httpChannel.requestMethod == 'GET' ? undefined : '#EEEEEE'};
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}

			if (httpChannel.isMainDocumentChannel)
				msg[(lh.barCnt++) + '------'] = {name: 'MAIN DOCUMENT', val: 'MAIN DOCUMENT (MAIN or FRAME)'};

			if (isPrivateChannel)
			{
				msg[(lh.barCnt++) + '------'] = {name: 'private', val: 'Private channel'};
				lh.serviceData.isPrivateChannel = true;
			}
			else
				lh.serviceData.isPrivateChannel = false;

			if (!context)
				msg[(lh.barCnt++) + '------'] = {name: 'service', val: 'Service channel'};
		}

		if (modify && !created)
		{
			return lh;
		}

		if (!modify)
		{
			HTTPUACleaner.LogRequestLoadFlags(httpChannel, msg, lh);
			
			if (modify)
				msg[(lh.barCnt++) + '------'] = {name: '----------------', val: 'REQUEST'};
			else
				msg[(lh.barCnt++) + '------'] = {name: '----------------', val: 'PRE REQUEST'};
			msg[(lh.barCnt++) + '------'] = {name: '----------------', val: '----------------------------------------------------------------'};

			var visitor = function() {};
			visitor.prototype.visitHeader = function(header, value)
			{
				lh.headers.pre[header] = value;
				msg['(' + (lh.hcnt++) + ')'] = {name: header, val: value};
			};

			// Здесь те заголовки, которые предварительные
			// Настоящие можно получить из Response
			httpChannel.visitRequestHeaders(new visitor());
			msg[(lh.barCnt++) + '------'] = {name: '----------------', val: '----------------------------------------------------------------'};

			var truncLen = HTTPUACleaner.truncLenght.httplog.data;
			
			if (truncLen > 0)
			try
			{
				// https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/nsIUploadChannel
				var uploadChannel = subject.QueryInterface(Ci.nsIUploadChannel);
				if (uploadChannel.uploadStream)
				{
					var ss = uploadChannel.uploadStream.QueryInterface(Ci.nsISeekableStream);
					var scriptableStream = Cc['@mozilla.org/scriptableinputstream;1'].createInstance(Ci.nsIScriptableInputStream); //.getService(Ci.nsIScriptableInputStream);
					scriptableStream.init(ss);

					ss.seek(0, 0);
					var data = scriptableStream.read(scriptableStream.available());
					ss.seek(0, 0);

					var decodedData = false;
					try
					{
						decodedData = decodeURI(data);
					}
					catch (e)
					{}

					var truncated = false;
					if (data.length > truncLen)
					{
						data = data.substring(0, truncLen) + '...';
						truncated = true;
					}
					if (decodedData && decodedData.length > truncLen)
					{
						decodedData = decodedData.substring(0, truncLen) + '...';
						truncated = true;
					}

					msg[(lh.barCnt++) + '------'] = {name: truncated ? 'data (truncated)' : 'data', val: data, noDecode: true, maxLen: truncLen};
					if (decodedData && decodedData != data)
					msg[(lh.barCnt++) + '------'] = {name: truncated ? 'data (decoded) (truncated)' : 'data (decoded)', val: decodedData, noDecode: true, maxLen: truncLen};
				}
			}
			catch (e)
			{
				msg[(lh.barCnt++) + '------'] = {name: 'data (error)', val: e.message, noDecode: true};
				HTTPUACleaner.logObject(e, true);;
			}
		}

		HTTPUACleaner.loggerB.addToLog(taburl, false, httpChannel.URI.spec, {type: 'http log', level: 11, msg: msg});
		HTTPUACleaner.loggerH.addToLog('',     false, httpChannel.URI.spec, {type: 'http log', level: 11, msg: msg, serviceData: lh.serviceData});

		return lh;
	},

	LogResponseHeaders: function(subject, httpChannel, taburl, host, obj, context, modify, isPrivateChannel)
	{
		var lh;
		for (var hn in HTTPUACleaner.LogHeaders)
		{
			lh = HTTPUACleaner.LogHeaders[hn];
			if (lh.subject == subject)
				break;

			lh = null;
		}

		if (!lh)
		{
			if (taburl === 0)
			{
				HTTPUACleaner.logMessage('HUAC ERROR: LogResponseHeaders not found for response ' + httpChannel.URI.spec + ' and response skipped from log', true);
				return;
			}

			lh = HTTPUACleaner.LogRequestHeaders(subject, httpChannel, taburl, host, obj, context, true, isPrivateChannel);

			if (Date.now() - HTTPUACleaner.startupTime > 15000)
				HTTPUACleaner.logMessage('HUAC ERROR: LogResponseHeaders not found for response ' + httpChannel.URI.spec, true);
		}

		var msg = lh.msg;

		if (!msg)
		{
			HTTPUACleaner.logMessage('HUAC ERROR: LogResponseHeaders not found for response ' + httpChannel.URI.spec + ' and can not create internal data', true);
			return;
		}

		let getReceived = function(lh)
		{
			var toTime = function(time)
			{
				if (time < 10000)
					return '' + time + 'ms';

				if (time < 60000)
					return '' + Math.round(time/100)/10.0 + 's';

				return '' + Math.round(time/1000/6)/10.0 + 'm';
			};

			var time = (lh.serviceData.stop - lh.serviceData.start);

			time = ' ' + toTime(time);

			if (lh.serviceData.startD)
			{
				var time2 = lh.serviceData.stop - lh.serviceData.startD;
				time2 = ' ' + toTime(time2) + ' /' + time;
				
				return time2;
			}
			else
			{
				return time;
			}
		};

		if (!lh.registeredListener && modify !== 0)
		{
			let StartStopListener = function()
			{
				// https://dxr.mozilla.org/mozilla-beta/source/xpcom/io/nsIStorageStream.idl
				this.ss = Cc["@mozilla.org/storagestream;1"].createInstance(Ci.nsIStorageStream);
				this.ss.init(Math.pow(2, 16), 0xffffffff, null);
				this.bos = Cc['@mozilla.org/binaryoutputstream;1'].createInstance(Ci.nsIBinaryOutputStream);
				this.bos.setOutputStream(this.ss.getOutputStream(0));
			};

			StartStopListener.prototype =
			{
				QueryInterface: XPCOMUtils.generateQI([Ci.nsIRequestObserver, Ci.nsIStreamListener/*, Ci.nsISupports*/]),

				onStartRequest: function(request, context)
				{
					lh.serviceData.stopped = false;

					if (this.originalListener)
					this.originalListener.onStartRequest(request, context);
				},

				onStopRequest: function(request, context, statusCode)
				{
					var truncLen  = HTTPUACleaner.truncLenght.httplog.info;
					var truncLenI = HTTPUACleaner.truncLenght.httplog.infoI;
					lh.serviceData.stopped = true;
					lh.serviceData.stop    = Date.now();
					msg.received  = '' + lh.received + ' (finished in ' + getReceived(lh) + ')';

					if (truncLen > 0 || truncLenI > 0)
					try
					{
						var cntCharset = null;
						try
						{
							// Иногда даёт NS_ERROR_NOT_AVAILABLE
							cntCharset = request.contentCharset ? request.contentCharset : null;
						}
						catch (e)
						{}

						var nis = this.ss.newInputStream(0);
						let is = Cc["@mozilla.org/intl/converter-input-stream;1"].createInstance(Ci.nsIConverterInputStream);
						is.init(nis, cntCharset, -1, Ci.nsIConverterInputStream.DEFAULT_REPLACEMENT_CHARACTER);

						var data = [];
						var count = this.ss.length;
						while (count)
						{
							let str = {};
							let bytesRead = is.readString(count, str);
							if (!bytesRead)
								break;

							count -= bytesRead;
							data.push(str.value);
						}
						data = data.join('');
						nis.close();

						var decodedData;
						try
						{
							decodedData = decodeURI(data);
						}
						catch (e)
						{}

						var truncated = false;
						if (data.length > truncLen)
						{
							data = data.substring(0, truncLen) + '...';
							truncated = true;
						}

						lh.info = {name: truncated ? 'information (truncated)' : 'information', val: data, noDecode: true, maxLen: truncLen};
						if (this.dt && httpChannel.contentType.startsWith('image/'))
						{
							this.dt = this.dt.join('');
							if (this.dt.length < truncLenI)
							{
								lh.info.img = 'data:' + request.contentType + ';base64,' + HTTPUACleaner.base64.encode(this.dt);
							}
						}
						else
						if (decodedData && decodedData != data)
						{
							if (decodedData.length > truncLen)
							{
								decodedData = decodedData.substring(0, truncLen) + '...';
								truncated = true;
							}

							lh.infoD = {name: truncated ? 'information (truncated) (decoded)' : 'information (decoded)', val: decodedData, noDecode: true, maxLen: truncLen};
						}

						delete this.dt;

						this.ss .close();
						this.bos.close();
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}

					try
					{
						HTTPUACleaner.deleteRequestFromArray();
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}

					if (this.originalListener)
					this.originalListener.onStopRequest(request, context, statusCode);
				},

				// Данные могут передаваться повторно
				onDataAvailable: function(aRequest, context, aInputStream, aOffset, aCount)
				{
					var truncLen  = HTTPUACleaner.truncLenght.httplog.info;
					var truncLenI = HTTPUACleaner.truncLenght.httplog.infoI;
					var newS;

					lh .received += aCount;
					msg.received  = '' + lh.received + ' (in progress)';

					if (truncLen > 0 || truncLenI > 0)
					try
					{
						if (!this.dt && this.dt !== false)
						{
							this.dt = [];
							this.dtlen = 0;
						}

						var bis = Cc["@mozilla.org/binaryinputstream;1"].createInstance(Ci.nsIBinaryInputStream);
						bis.setInputStream(aInputStream);

						var position = this.ss.length;
						var data = bis.readBytes(aCount);
						this.bos.writeBytes(data, data.length);
						this.dtlen += aCount;

						if (data.length > 0)
						{
							if (this.dtlen < truncLenI)
								this.dt.push(data);
							else
								this.dt = false;
						}

						// Если использовать тот же самый поток storagestream, он там чего-то, похоже, хочет быть неблокирующим
						var tmpStorage = Cc["@mozilla.org/storagestream;1"].createInstance(Ci.nsIStorageStream);
						tmpStorage.init(Math.pow(2, 16), aCount, null);
						var tmpBos = Cc['@mozilla.org/binaryoutputstream;1'].createInstance(Ci.nsIBinaryOutputStream);
						tmpBos.setOutputStream(tmpStorage.getOutputStream(0));
						tmpBos.writeBytes(data, data.length);

						newS = tmpStorage.newInputStream(0);
						tmpBos.close();
						tmpStorage.close();
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}

					try
					{
						if (this.originalListener)
						if (newS)
						{
							this.originalListener.onDataAvailable(aRequest, context, newS, aOffset, aCount);
							newS.close();
						}
						else
							this.originalListener.onDataAvailable(aRequest, context, aInputStream, aOffset, aCount);
					}
					catch (e)
					{
						if (e.name != 'NS_BINDING_ABORTED')
						{
							HTTPUACleaner.logMessage('HUAC ERROR in the http log response listener with uri ' + aRequest.URI.spec + ' in tab ' + taburl, true);
							console.error(e.message);
							HTTPUACleaner.logObject(e, true);;
						}
					}
				}
			};

			try
			{
				let tracebleChannel = subject.QueryInterface(Ci.nsITraceableChannel);
				let ssl = new StartStopListener();
				ssl.originalListener = tracebleChannel.setNewListener(ssl);
				lh.registeredListener = true;
				lh.serviceData.startD = Date.now();
			}
			catch (e)
			{
				if (Date.now() - HTTPUACleaner.startupTime > 15000)
				{
					lh.info = {name: 'info (error; ' + modify + ')', val: e.message, bgcolor: '#FF0000'};

					HTTPUACleaner.logMessage('HUAC ERROR: LogResponseHeaders for URI ' + subject.URI.spec, true);
					HTTPUACleaner.logObject(e, true);
					HTTPUACleaner.logMessage('modify = ' + modify, true);
				}
				else
					lh.info = {name: 'info (error; ' + modify + ')', val: '(HUAC not initialized?) ' + e.message, bgcolor: '#FF000022'};
			}
		}

		var cached = false;
		if (modify !== 0)
			cached = (modify == "2BROHTPcf2RtJzXuCyrwhHKZgT7") || (modify == "ImLYE2mChLYJvHcV9EvuPYwV1B9");
/*
		msg = {'status': '' + httpChannel.responseStatus + ' ' + httpChannel.responseStatusText, 'url': httpChannel.requestMethod + ' ' + (httpChannel.URI ? httpChannel.URI.spec : ''), 'private/service': isPrivateChannel + '/' + (!context), 'tab': taburl, '------': '----------------------------------------------------------------'};
*/


		var visitor = function() {};
		visitor.prototype.visitHeader = function(header, value)
		{
			var color, bgcolor;
			if (!lh.headers.pre[header])
				color = '#0000FF';
			else
			if (lh.headers.pre[header] != value)
				color = '#FF0000';

			lh.headers.r[header] = value;

			msg['(' + (lh.hcnt++) + ')'] = {name: header, val: value, color: color, bgcolor: bgcolor};
		};

		if (lh.requestHeadersGetted !== true)
		{
			msg[(lh.barCnt++) + '------'] = {name: '----------------', val: 'REQUEST'};
			msg[(lh.barCnt++) + '------'] = {name: '----------------', val: '----------------------------------------------------------------'};
			
			httpChannel.visitRequestHeaders(new visitor());
			lh.requestHeadersGetted = true;
		}

		msg[(lh.barCnt++) + '------'] = {name: '----------------', val: '----------------------------------------------------------------'};

		if (cached)
		{
			lh.cached = true;
			lh.serviceData.cached = true;
		}

		if (lh.responseStatusGetted !== true)
		try
		{
			var httpstatus = {name: 'RESPONSE', val: '' + httpChannel.responseStatus + ' ' + httpChannel.responseStatusText, bgcolor: httpChannel.requestSucceeded ? undefined : '#FFFF0033'};
			if (httpChannel.responseStatus >= 400)
				httpstatus.bgcolor = '#FF0000';

			lh.serviceData.responseStatus 	  = httpChannel.responseStatus;
			lh.serviceData.responseStatusText = httpChannel.responseStatusText;
			if (cached)
			{
				if (httpChannel.responseStatus == 200)
				{
					httpstatus.val += ' (cached)';
					if (!httpstatus.bgcolor)
						httpstatus.bgcolor = '#FFFF0033';
				}
			}

			msg[(lh.barCnt++) + '------'] = httpstatus;

			lh.responseStatusGetted = true;
		}
		catch (e)
		{
			if (httpChannel.status == 0)
				HTTPUACleaner.logObject(e, true);;
		}

		if (modify !== 0)
		{
			/* Может быть h2 и т.п.
			if (httpChannel.protocolVersion.indexOf('http') < 0)
			{
				try
				{
					console.error(httpChannel);
				}
				catch (e)
				{}
			}*/
			try
			{
				msg[(lh.barCnt++) + '------'] = {name: 'Protocol', val: httpChannel.protocolVersion + ' (' + httpChannel.URI.scheme + ')'};
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}

			try
			{
				lh.serviceData.TLS = false;
				var si = HTTPUACleaner.logger.getSecurityInfo(httpChannel);
				if (si instanceof Ci.nsISSLStatusProvider)
				{
					var TLSState = si.SSLStatus;
					TLSState.QueryInterface(Ci.nsISSLStatus);

					var TLS_cs = TLSState.secretKeyLength + ' ' + TLSState.cipherName;

					switch (TLSState.protocolVersion)
					{
						case 0: msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'SSL 3.0 ' + TLS_cs, bgcolor: '#FF0000'};
							lh.serviceData.TLS = 0;
							break;
						case 1: msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'TLS 1.0 ' + TLS_cs, bgcolor: '#FFFF00'};
							lh.serviceData.TLS = 0;
							break;
						case 2: msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'TLS 1.1 ' + TLS_cs};
							lh.serviceData.TLS = 0;
							break;
						case 3: msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'TLS 1.2 ' + TLS_cs};
							lh.serviceData.TLS = true;
							break;
						case 4: msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'TLS 1.3 ' + TLS_cs};
							lh.serviceData.TLS = true;
							break;
						default: msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'TLS version unknown ' + TLS_cs};
							break;
					}

					if (TLSState.isUntrusted)
					{
						msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'UNTRUSTED', bgcolor: '#FF0000'};
						lh.serviceData.TLS = 0;
					}
					if (TLSState.isNotValidAtThisTime)
					{
						msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'EXPIRED', bgcolor: '#FF0000'};
						lh.serviceData.TLS = 0;
					}
					if (TLSState.isDomainMismatch)
					{
						msg[(lh.barCnt++) + '------'] = {name: 'TLS', val: 'DOMAIN MISMATCH', bgcolor: '#FF0000'};
						lh.serviceData.TLS = 0;
					}
				}
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}

			if (isPrivateChannel)
				msg[(lh.barCnt++) + '------'] = {name: 'private', val: 'Private channel'};
			if (!context)
				if (context === 0)
					msg[(lh.barCnt++) + '------'] = {name: 'service', val: 'undefined'};
				else
					msg[(lh.barCnt++) + '------'] = {name: 'service', val: 'Service channel'};

			HTTPUACleaner.LogRequestLoadFlags(httpChannel, msg, lh);
		}
		else
		{
			msg['info']  = lh.info;
			if (lh.infoD)
			msg['infoD'] = lh.infoD;

			if (subject.transferSize + subject.encodedBodySize + subject.decodedBodySize > 0)
				msg[(lh.barCnt++) + '------'] = {name: '----------------', val: '----------------------------------------------------------------'};

			if (subject.transferSize)
			{
				if (lh.cached)
				{
					lh.serviceData.validated = true;
					msg[(lh.barCnt++) + '------'] = {name: 'cache', val: 'cached, validated', bgcolor: '#FFFF0033'};
				}
				msg[(lh.barCnt++) + '------'] = {name: 'transferSize', val: '' + subject.transferSize};
				lh.serviceData.transferSize = subject.transferSize;
			}
			else
			{
				if (lh.cached)
					msg[(lh.barCnt++) + '------'] = {name: 'cache', val: 'cached', bgcolor: '#FFFF0033'};
			}

			if (subject.encodedBodySize)
			{
				
				if (subject.decodedBodySize)
				{
					msg[(lh.barCnt++) + '------'] = {name: 'encodedBodySize', val: '' + subject.encodedBodySize + ' (' + (Math.round(subject.encodedBodySize*100/subject.decodedBodySize)) + '%)'};
				}
				else
					msg[(lh.barCnt++) + '------'] = {name: 'encodedBodySize', val: '' + subject.encodedBodySize};
			}
			if (subject.decodedBodySize)
				msg[(lh.barCnt++) + '------'] = {name: 'decodedBodySize', val: '' + subject.decodedBodySize};

			// nsIHttpChannelInternal
			try
			{
				if (subject.remoteAddress)
				{
					msg[(lh.barCnt++) + '------'] = {name: 'address', val: '' + subject.remoteAddress + ':' + subject.remotePort + ' <-> ' + subject.networkInterfaceId + ' ' + subject.localAddress + ':' + subject.localPort};
				}
			}
			catch (e)
			{
				if (!lh.cached && httpChannel.status != 0)
				{
					HTTPUACleaner.logMessage('HUAC hard warning: remotedAddress (or other fields) in LogResponseHeaders is anavailable', true);
					HTTPUACleaner.logMessage(subject.URI.spec, true);
					HTTPUACleaner.logObject(e, true);
				}
			}

			lh.serviceData.canceled = subject.canceled;
		}

		if (modify === 0 && httpChannel.status != 0)
		{
			lh.serviceData.status = httpChannel.status;

			if (httpChannel.status == Cr.NS_BINDING_REDIRECTED)
			{
				msg[(lh.barCnt++) + '------'] = {name: 'channel status', val: 'NS_BINDING_REDIRECTED' + ' (' + httpChannel.status.toString(16).toUpperCase() + 'h)', bgcolor: '#FFFF0033'};

				lh.serviceData.redirected = true;
				lh.serviceData.statusText = 'NS_BINDING_REDIRECTED';
				
				if (lh.serviceData.stopped !== true)
				{
					lh.serviceData.stopped = true;
					lh.serviceData.stop    = Date.now();
					msg.received  = '' + lh.received + ' (redirected in ' + getReceived(lh) + ')';
				}
			}
			else
			{
				var flag = httpChannel.status;
				for (var crName in Cr)
				{
					if (Cr[crName] == httpChannel.status)
					{
						flag = crName;
						break;
					}
				}

				msg[(lh.barCnt++) + '------'] = {name: 'channel status', val: flag + ' (' + httpChannel.status.toString(16).toUpperCase() + 'h)', bgcolor: '#FF0000'};
				lh.serviceData.statusText = flag;

				if (lh.serviceData.stopped !== true)
				{
					lh.serviceData.stopped = 1;
					lh.serviceData.stop    = Date.now();
					msg.received  = '' + lh.received + ' (error in ' + getReceived(lh) + ')';
				}
			}
		}
		else
		if (modify === 0)
		{
			if (lh.serviceData.stopped !== true)
			{
				lh.serviceData.stopped = 1;
				lh.serviceData.stop    = Date.now();
				msg.received  = '' + lh.received + ' (? in ' + getReceived(lh) + ')';
			}
		}

		if (lh.responseHeadersGetted !== true)
		{
			msg[(lh.barCnt++) + '------'] = {name: '----------------', val: '----------------------------------------------------------------'};
			try
			{
				var respCnt = 0;
				visitor = function() {};
				visitor.prototype.visitHeader = function(header, value)
				{
					lh.headers.resp['' + (respCnt++)] = [header, value];
					msg['(' + (lh.hcnt++) + ')'] = {name: header, val: value};

					if (header.toLowerCase() == 'content-type')
						if (lh.serviceData['content-type'])
							lh.serviceData['content-type'] += ' !ERROR! ' + value;
						else
							lh.serviceData['content-type'] = value;
				};

				//httpChannel.visitResponseHeaders(new visitor());
				httpChannel.visitOriginalResponseHeaders(new visitor());

				lh.responseHeadersGetted = true;
			}
			catch (e)
			{
				msg[(lh.barCnt++) + '------'] = {name: 'ERROR', val: 'NO RESPONSE HEADERS'};
			}
		}

		if (modify === 0)
			delete HTTPUACleaner.LogHeaders[lh.cnt];
/*
		try
		{
			console.error(subject);
		}
		catch (e)
		{}*/
	},
	
	onHttpRequestOpened: function (subject, modify)
	//onHttpRequestModify: function (subject, aTopic, aData)
	{
		if (!HTTPUACleaner.enabled)
			return;

		var httpChannel = subject.QueryInterface(Ci.nsIHttpChannel);

/*
		if (httpChannel.loadGroup && httpChannel.loadGroup.notificationCallbacks)
		{
			var callback = httpChannel.loadGroup.notificationCallbacks; //httpChannel.notificationCallbacks;
			console.error
			(
				callback.getInterface(Ci.nsIWebNavigation)
					.QueryInterface(Ci.nsIDocShellTreeItem)
			);
		}
*/

		var logMessages = [];
		var requestBlocked = false;

		var obj		 = {onHttpRequestOpened: true};
		var context  = HTTPUACleaner.getHttpContext(subject, httpChannel);
		var host     = HTTPUACleaner.getHostName(httpChannel, subject, modify ? true : false, context, obj);
		var protocol = HTTPUACleaner.getProtocolFromURL(httpChannel.URI.spec);

		if (
			   protocol == "data:"
			|| protocol == "about:"
			|| protocol == "moz-safe-about:"
			|| protocol == "moz-filedata:"
			|| protocol == "moz-icon:"
			|| protocol == "mediasource:"
			|| protocol == "blob:"
		)
		{
			return;
		}

		var isPrivateChannel = false;
		try
		{
			try
			{
				var s = subject.QueryInterface(Ci.nsIPrivateBrowsingChannel);
				isPrivateChannel = s.isChannelPrivate;
			}
			catch (e)
			{
				isPrivateChannel = false;
				HTTPUACleaner.logObject(e, true);
			}

			// Почему-то такое бывает в response,
			// например, когда фильтр Images грузит с приватного режима картинки с сайта http://kartinki.ru/
			// Здесь стоит на всякий случай
			if (context && !isPrivateChannel && context.usePrivateBrowsing)
			{
				s.setPrivate(true);
				isPrivateChannel = true;
			}
		}
		catch (e)
		{
			isPrivateChannel = false;
			HTTPUACleaner.logObject(e, true);;
		}

		// modify вызывается позже
		if (HTTPUACleaner.httplog.enabled && !modify)
		{
			HTTPUACleaner.LogRequestHeaders(subject, httpChannel, obj.taburl, host, obj, context, modify, isPrivateChannel);
		}


		if (HTTPUACleaner.getFunctionState(host, "NoFilters") == 'no filters')
		{
			return;
		}

		var isOCSP = HTTPUACleaner.isOCSPRequest(httpChannel);

		if (!isOCSP && (protocol == 'https:' || protocol == 'wss:'))
		{	
			let hstTmp = HTTPUACleaner.getHostByURI(httpChannel.URI.spec);

			HTTPUACleaner.certsObject.rootCertificateEnable
			(
				HTTPUACleaner.getDomainByHost(hstTmp), hstTmp, 2, httpChannel, obj.taburl
			);
/*
			if (HTTPUACleaner.certsObject.hostsOpt.autodisable)
			{
				httpChannel.setRequestHeader("Connection", 	'Close', false);
			}*/
		}

		var redirectToHTTPS = function(addToLog)
		{
			if (httpChannel.URI.scheme == 'https' || httpChannel.URI.scheme == 'wss')
				return true;

			if (!isOCSP)
			{
				var redirectSucceded = false;
				// Для редиректов там, где есть ссылка, например, на файл по протоколу http, а нужно по https
				// Из Content-Policy должны дойти только запросы с contentType == Ci.nsIContentPolicy.TYPE_DOCUMENT (??? о чём это?)
				// Например, https://meduza.io/feature/2015/06/15/zabvenie-prava-na-poisk
				try
				{
					var a = httpChannel.URI.spec;
					if (a.substr(0, 5) == "http:")
					{
						var ioService = HTTPUACleaner.ioService;
						var uri = ioService.newURI('https:' + a.substr(5), httpChannel.URI.OriginCharset, null);
						httpChannel.redirectTo(uri);

						if (obj.huac && obj.huac.url == a)
						{
							obj.huac.url = uri.spec;
						}

						if (obj.homeFound && obj.homeFound.url)
							obj.homeFound.url = uri.spec;	// Это для того, чтобы не испортить массив lastUrls непредусмотренным редиректом

						
						if (addToLog)
						HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Only HTTPS', level: 5, msg: {'source': 'http request', action: HTTPUACleaner.words['redirected'], tab: obj.taburl/*, host: host, requestHost: HTTPUACleaner.getHostByURI(httpChannel.URI.spec), hostState: HTTPUACleaner.getFunctionState(host, "OnlyHttps")*/}});

						redirectSucceded = true;
					}
				}
				catch (e)
				{
					// HTTPUACleaner.logObject(e, true);;
				}
				
				if (redirectSucceded !== true)
				{
					httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
					
					if (addToLog)
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Only HTTPS', level: 3, msg: {'source': 'http request', action: HTTPUACleaner.words['blocked'], tab: obj.taburl}});

					requestBlocked = true;
					return redirectSucceded;
				}
				
				return redirectSucceded;
			}
			else
			{
				if (addToLog)
				HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Only HTTPS', level: 4, msg: {'source': 'http request', type: 'ocsp-request', action: HTTPUACleaner.words['allowed'], tab: obj.taburl}});

				return false;
			}
		};

		var isOnlyHttpsHost   = HTTPUACleaner.isOnlyHttps(host, HTTPUACleaner.getHostByURI(httpChannel.URI.spec));
		var redirectedToHTTPS = false;
		if (  isOnlyHttpsHost && (protocol != 'https:' && protocol != 'wss:')  )
		{
			try
			{
				redirectedToHTTPS = redirectToHTTPS(true);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}
		}

		let f = {iCookies: 0};

		// Работа правил вкладки Side
		SideWork:
		try
		{
			TLInfo = {f: true};

			TLInfo.isOCSP       = isOCSP;
			TLInfo.minTLSStrong = HTTPUACleaner.minTLSStrong;
			TLInfo.haveContext  = !!context;

			TLInfo.isPrivate = isPrivateChannel;
				// например, когда фильтр Images грузит с приватного реж��ма картинки с сайта http://kartinki.ru/

			if (!HTTPUACleaner.loggerSide.enabled)
				break SideWork;

			TLInfo.origin = null;
			try
			{
				TLInfo.origin = httpChannel.getRequestHeader("Origin");
			}
			catch (e)
			{
			}
			
			// obj.taburl obj.redirectTo
			obj.turl = obj.taburl ? obj.taburl : httpChannel.URI.spec;
			f = HTTPUACleaner.sdb.checkRules.response.bind(HTTPUACleaner.sdb)(httpChannel.URI.spec, obj, {rtype: undefined, ftype: undefined}, TLInfo, modify ? 'http request' : 'http request (pre)');

			if (f.cookie === true)
			{
				httpChannel.setRequestHeader("Cookie", 	undefined, false);
				// console.error('Cookie cleared for ' + httpChannel.URI.spec);
			}

			if (f.cancel === true)
			{
				// Если это изображение, загружаемое по клику, то разрешить загрузку
				if (HTTPUACleaner.searchPassImg(httpChannel.URI.spec))
				{
					f.cancel = false;
					f.log.msg['image allowed'] = 'allowed by load image by click';
				}
				else
				{
					httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);

					// Фреймы, к сожалению, тоже редиректятся
					// Редиректы не попадают в Content-Policy, поэтому редиректить на страницу сообщения о блокировке приходится здесь
					// context.isTopLevel в этот момент времени, видимо, ещё не установлен, т.к. потом он true, а в этом обработчике - undefined
					if (httpChannel.isMainDocumentChannel && obj.taburl == httpChannel.URI.spec)
					{
						try
						{
							// Простой редирект не работает
							if (context && HTTPUACleaner.urls.getTabForContext)
							{
								var ioService = HTTPUACleaner.ioService;
								var newUrl = HTTPUACleaner['sdk/self'].data.url('blocked.html') + '?' + httpChannel.URI.spec;
								var uri = ioService.newURI(newUrl, httpChannel.URI.OriginCharset, null);

								var sdkTab = HTTPUACleaner.urls.chromeTabToSdkTab(HTTPUACleaner.urls.getTabForContext(context));
								if (sdkTab)
									sdkTab.url = uri.spec;
								else
									console.error('HUAC warning: sdkTab in tab redirect is null for ' + httpChannel.URI.spec);
							}
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);;
						}
					}/*
					else
					{
						if (httpChannel.isMainDocumentChannel)
						{
							console.error('no tab redirect for ' + httpChannel.URI.spec);
							console.error(obj.taburl);
							console.error(obj);
						}
					}*/

					// Здесь нельзя ставить return, т.к. логирование происходит позже
				}
			}
			else
			if (f.toHttps === true && !redirectedToHTTPS)
			{
				redirectedToHTTPS = redirectToHTTPS(true);
				// console.error('Cookie cleared for ' + httpChannel.URI.spec);
			}

			if (f.executed === true || f.log.level != 1)
			HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, f.log);

			if (f.cancel === true)
			{
				return;
			}

			if (f.toHttps === true && !redirectedToHTTPS)
			{
				return;
			}
		}
		catch (e)
		{
			console.error("FATAL ERROR");
			console.error("HTTPUACleaner.sdb.checkRules.response in request has raised exception");
			console.error(uri);
			console.error(obj.redirectTo);
			console.error(httpChannel.URI.spec);
			HTTPUACleaner.logObject(e, true);;
		}

		// HTTPUACleaner.loggerSide.addToLog(obj.taburl, obj.redirectTo ? obj.redirectTo : false, httpChannel.URI.spec, host, {type: 'request'});

		var hostPP = (TLInfo.isPrivate ? ':' : '') + host;
		var domain_for_cookies = HTTPUACleaner.getDomainByHost(host);
		if (TLInfo.isPrivate)
			domain_for_cookies = ':' + domain_for_cookies;


		// User-Agent functional
		var state		 = HTTPUACleaner.getFunctionState(host, "UA");
		var statem		 = HTTPUACleaner.getFunctionState(host, "MUA");

		// создаёт HTTPUACleaner.HostOptions[hostPP] с CacheControl, ����сли его не было
		var ua = HTTPUACleaner.getEtalonNavigatorObject(state, hostPP, statem, host).userAgent;
		if (state != "disabled" || statem != "disabled")
		{
			httpChannel.setRequestHeader("User-Agent", ua, false);
		}


		var iCookiesSet = function(regime, hRegime)
		{
			var getCookieStr = function(hostPPA, hostA, domain_for_cookies)
			{
				HTTPUACleaner.generateNewiCookieRandomStrIfNotSet(hostPPA, hostA, regime, domain_for_cookies);

				var rndStr = HTTPUACleaner.generateCookieRndStr(TLInfo.isPrivate, TLInfo.haveContext, regime, hostPPA, domain_for_cookies);

				var newUri  = httpChannel.URI.clone();
				if (regime > 0)
					newUri.host = httpChannel.URI.host + '.' + rndStr + '.huac';

				var str = '';
				if (regime > 0)
				{
					if (httpChannel.URI.spec.substr(0, 6) == 'https:' || httpChannel.URI.spec.substr(0, 4) == 'wss:')
						str = HTTPUACleaner.cs.getCookieStringFromHttp(newUri, null, /*httpChannel*/HTTPUACleaner.privateChannelS);
					else
						str = HTTPUACleaner.cs.getCookieStringFromHttp(newUri, null, /*httpChannel*/HTTPUACleaner.privateChannelN);
				}
				else
				if (hRegime == 2)
				{
					str = HTTPUACleaner.cs.getCookieStringFromHttp(newUri, null, httpChannel);
				}

				return str;
			};

			var str = getCookieStr(hostPP, host, domain_for_cookies);

			// Убран��, т.к. изменён механизм изоляции куков на HTTPUACleaner.CookieRandomStrDomain[domain_for_cookies].(Iso/Rnd)
			// Изоляция Id или +
			// Режим 3 - Ih тут не нужен, т.к. режим 3 изолирует хост, а не домен
			/*if (regime == 1 || regime == 2)
			{
				var stra = [];
				if (str)
					stra.push(str);

				var domain = '.' + HTTPUACleaner.getDomainByHost(host);

				for (var h in HTTPUACleaner.HostOptions)
				{
					if (TLInfo.isPrivate && !h.startsWith(':'))
						continue;
					if (!TLInfo.isPrivate && h.startsWith(':'))
						continue;

					if (h.endsWith(domain))
					{
						var hp = h;
						if (hp.startsWith(':'))
							hp = hp.substring(1);

						var tmp = getCookieStr(h, hp);
						if (tmp)
							stra.push(tmp);
					}
				}
				
				var ck = {};
				for (var strb of stra)
				{
					if (!strb)
						continue;

					strb = strb.split(';');

					for (var strc of strb)
					{
						var i = strc.indexOf('=');
						var name = strc.substring(0, i);
						var val  = strc.substring(i + 1);

						// Если хост hostPP уже установил куку, то мы здесь ничего не предпринимаем
						// По��ому что changeCookie, вообще говоря, не совсем верно работает и не изменяет куки на другие хосты
						if (ck[name] === undefined)
							ck[name] = val;
					}
				}
				
				var ckstr = '';
				for (var cka in ck)
				{
					if (ckstr.length > 0)
						ckstr += ';';

					ckstr += cka + '=' + ck[cka];
				}

				str = ckstr;
			}*/

			if (!str)
				str = null;

			httpChannel.setRequestHeader("Cookie", 	str, false);
		};

		if (f.iCookies > 0 || f.cookie == 2)
		{
			try
			{
				httpChannel.setRequestHeader("Cookie", 	undefined, false);
				iCookiesSet(f.iCookies, f.cookie);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
				httpChannel.setRequestHeader("Cookie", 	undefined, false);
			}
		}


		var Referer = function(state)
		{
			if ((state == "host" || state == 'host only') && !host.noTab)
			{
				try
				{
					var rc = httpChannel.getRequestHeader("Referer");

					if (rc && HTTPUACleaner.getHostByURI(rc) != HTTPUACleaner.getHostByURI(httpChannel.URI.spec))
					{
						httpChannel.setRequestHeader("Referer", undefined, false);
						logMessages.push({name: 'Referer', state: state});
					}
				}
				catch (e)
				{
					httpChannel.setRequestHeader("Referer", undefined, false);

					if (e.result != 2147746065 && !TLInfo.isPrivate)		// Если просто нет заголовка - ничего не выводить
					{
						logMessages.push({name: 'Referer', state: 'error/' + state});
						HTTPUACleaner.logObject(e, true);;
					}
					else
						logMessages.push({name: 'Referer', state: state + '/no referer'});
				}
			}
			else
			if ((state == "domain") && !host.noTab)
			{
				try
				{
					var rc = httpChannel.getRequestHeader("Referer");

					if (rc && HTTPUACleaner.getDomainByHost(HTTPUACleaner.getHostByURI(rc)) != HTTPUACleaner.getDomainByHost(HTTPUACleaner.getHostByURI(httpChannel.URI.spec)))
					{
						httpChannel.setRequestHeader("Referer", undefined, false);
						logMessages.push({name: 'Referer', state: state});
					}
				}
				catch (e)
				{
					httpChannel.setRequestHeader("Referer", undefined, false);
					if (e.result != 2147746065 && !TLInfo.isPrivate)		// Если просто нет заголовка - ничего не выводить
					{
						logMessages.push({name: 'Referer', state: 'error/' + state});
						HTTPUACleaner.logObject(e, true);;
					}
					else
						logMessages.push({name: 'Referer', state: state + '/no referer'});
				}
			}
			else
			{
				httpChannel.setRequestHeader("Referer", undefined, false);
				logMessages.push({name: 'Referer', state: state});
			}
		};

		// https://learn.javascript.ru/xhr-crossdomain
		// http://habrahabr.ru/post/114432/
		var CORSFunc = function(state)
		{
			var origin = null;
			
			try
			{
				origin = httpChannel.getRequestHeader("Origin");
			}
			catch (e)
			{
				return;
			}

			if (state == "clean")
			{
				try
				{
					httpChannel.setRequestHeader("Cookie", 	undefined, false);
					httpChannel.setRequestHeader("Referer", undefined, false);

					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'CORS', level: 2, msg: {'origin': origin, action: 'clean', tab: obj.taburl}});
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
					httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
					requestBlocked = true;

					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'CORS', level: 3, msg: {'origin': origin, action: HTTPUACleaner.words['blocked'], state: 'error', tab: obj.taburl}});
				}
			}
			else
			if (state == 'domain')
			{
				if (HTTPUACleaner.getDomainByHost(host) != HTTPUACleaner.getDomainByHost(HTTPUACleaner.getHostByURI(httpChannel.URI.spec)))
				{
					httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);

					requestBlocked = true;
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'CORS', level: 2, msg: {'origin': origin, action: HTTPUACleaner.words['blocked'], state: state, tab: obj.taburl}});
				}
				/*else
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'CORS', msg: {'origin': origin, action: 'allowed', state: state}});*/
			}
			else
			{
				httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
				requestBlocked = true;
				
				HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'CORS', level: 2, msg: {'origin': origin, action: HTTPUACleaner.words['blocked'], tab: obj.taburl}});
			}
		};

		var DNT = function(state, recursive)
		{
			if (state == 'track')
			{
				httpChannel.setRequestHeader('DNT', "0", false);
				if (!recursive)
				logMessages.push({name: 'DNT', state: state + ' (0)'});
			}
			else
			if (state == 'no track')
			{
				httpChannel.setRequestHeader('DNT', "1", false);
				if (!recursive)
				logMessages.push({name: 'DNT', state: state + ' (1)'});
			}
			else
			if (state == 'clean')
			{
				httpChannel.setRequestHeader('DNT', undefined, false);
				if (!recursive)
				logMessages.push({name: 'DNT', state: state + ' (no send)'});
			}
			else
			if (state == 'random')
			{
				if
				(
						!HTTPUACleaner.HostOptions[hostPP]['DNT']
					|| 	 HTTPUACleaner.isHostTimeDied(hostPP, 'DNT', host) !== false
				)
				{
					HTTPUACleaner.HostOptions[hostPP]['DNT'] =
						{
							value: HTTPUACleaner.getRandomValueByArray(['no track', 'clean'])
						};
					HTTPUACleaner.setHostTime(hostPP, 'DNT', host);
				}

				DNT(HTTPUACleaner.HostOptions[hostPP]['DNT'].value, true);
				logMessages.push({name: 'DNT', state: state + ' (' + HTTPUACleaner.HostOptions[hostPP]['DNT'].value + ')'});
			}
		};
		
		var Locale = function(state)
		{
			if (state == 'enabled')
			{
				HTTPUACleaner.onDocumentLanguagesCals(hostPP, host, state);

				if (state == 'en-us')
				{
					httpChannel.setRequestHeader('Accept-Language', HTTPUACleaner.HostOptions[hostPP]['Accept-Language'].valENUS, false);
					logMessages.push({name: 'Locale', state: HTTPUACleaner.HostOptions[hostPP]['Accept-Language'].valENUS});
				}
				else
				{
					httpChannel.setRequestHeader('Accept-Language', HTTPUACleaner.HostOptions[hostPP]['Accept-Language'].value, false);
					logMessages.push({name: 'Locale', state: HTTPUACleaner.HostOptions[hostPP]['Accept-Language'].value});
				}
			}
		};

		var XForwardedFor = function(state)
		{
			var getIP = function()
			{
				return "" + HTTPUACleaner.getRandomInt(1, 223+1) + "." + HTTPUACleaner.getRandomInt(0, 254+1) + "." + HTTPUACleaner.getRandomInt(0, 254+1) + "." + HTTPUACleaner.getRandomInt(1, 254+1);
			};

			var gen = getIP();
			var lastIP = gen;
			if (state == "random")
			{

				if (
					!HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"] ||
					HTTPUACleaner.isHostTimeDied(hostPP, 'XForwardedFor', host) !== false ||
					HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"].st != state
					||
					HTTPUACleaner.isDied(  HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"].dateA, 17000  )		// если не было загрузки более 17 секунд, значит сменяем IP адрес, т.к. считаем что грузится уже следующая с��раница
					)
				{
					var L = 1;
					if (HTTPUACleaner.getRandomInt(0, 10) == 0)
						L = 3;

					var im = HTTPUACleaner.getRandomInt(0, L + 1);
					for (var i = im; i >= 0; i--)
					{
						var ip = getIP();
						gen += "," + ip;

						// http://www.iana.org/assignments/multicast-addresses/multicast-addresses.xhtml
						// http://www.iana.org/assignments/ipv4-address-space/ipv4-address-space.xhtml
						// http://datatracker.ietf.org/doc/rfc6761/?include_text=1
						// http://datatracker.ietf.org/doc/rfc1918/?include_text=1
						var localIP = [
									   /*"21.172.", "27.172.",
							"16.172.", "22.172.", "28.172.",
							"17.172.", "23.172.", "29.172.",
							"18.172.", "24.172.", "30.172.",
							"19.172.", "25.172.", "31.172.",
							"20.172.", "26.172.", "168.192.",*/
							 // адреса локальных сетей
							"192.168.", "10.",
							'172.16.',
							"172.17.",
							"172.18.",
							"172.19.",
							"172.20.",
							"172.21.",
							"172.22.",
							"172.23.",
							"172.24.",
							"172.25.",
							"172.26.",
							"172.27.",
							"172.28.",
							"172.29.",
							"172.30.",
							"172.31."/*,
							// спец. IP
							"224.",
							"239.",
							'0.',
							'255.'*/
						];

						for (var lip in localIP)
						{
							if (   ip.indexOf(  localIP[lip]  ) == 0   )
							{
								i = 0;
								break;
							}
						}

						if (i == 0 && HTTPUACleaner.getRandomInt(0, 1+1) == 0)
							lastIP = ip;
					}

					HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"] = {  gen: gen, last: lastIP, dateA: new Date(), st: state  };
					HTTPUACleaner.setHostTime(hostPP, 'XForwardedFor', host);
				}
				else
				{
					gen    = HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"].gen;
					lastIP = HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"].last;
					HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"].dateA = new Date();
				}
			}
			else
			{
				if (
					!HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"] ||
					HTTPUACleaner.isHostTimeDied(hostPP, 'XForwardedFor', host) !== false
					||
					HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"].st != state
					)
				{
					HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"] = {  gen: gen, last: lastIP, st: state  };
					HTTPUACleaner.setHostTime(hostPP, 'XForwardedFor', host);
				}
				else
				{
					gen = HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"].gen;
					lastIP = HTTPUACleaner.HostOptions[hostPP]["XForwardedFor"].last;
				}
			}

			httpChannel.setRequestHeader("X-Forwarded-For", gen, false);
			httpChannel.setRequestHeader("X-Real-IP", lastIP, false);
			httpChannel.setRequestHeader("Client-IP", lastIP, false);
			logMessages.push({name: 'X-Forwarded-For', state: state + ' (' + gen + ')'});
		};

		// https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/NsIRequest
		var noCacheRules = function(statel, statev)
		{
			if (statel == 0 && statev == 0)
				return;

			if (statev == 2)
			{
				httpChannel.loadFlags = httpChannel.loadFlags | Ci.nsIRequest.VALIDATE_ONCE_PER_SESSION;
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.VALIDATE_ALWAYS);
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.VALIDATE_NEVER);
			}
			else
			if (statev == 3)
			{
				httpChannel.loadFlags = httpChannel.loadFlags | Ci.nsIRequest.VALIDATE_ALWAYS;
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.VALIDATE_ONCE_PER_SESSION);
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.VALIDATE_NEVER);
			}
			else
			if (statev == 4)
			{
				httpChannel.loadFlags = httpChannel.loadFlags | Ci.nsIRequest.VALIDATE_NEVER;
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.VALIDATE_ALWAYS);
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.VALIDATE_ONCE_PER_SESSION);
			}
			
			if (statel == 2)
			{
				httpChannel.loadFlags = httpChannel.loadFlags | Ci.nsIRequest.LOAD_FROM_CACHE;
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.LOAD_BYPASS_CACHE);
			}
			else
			if (statel == 3)
			{
				httpChannel.loadFlags = httpChannel.loadFlags | Ci.nsIRequest.LOAD_BYPASS_CACHE;
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.LOAD_FROM_CACHE);
			}
			else
			if (statel == 0)
			{
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.LOAD_BYPASS_CACHE);
				httpChannel.loadFlags = httpChannel.loadFlags & (~Ci.nsIRequest.LOAD_FROM_CACHE);
			}
		};

		var noCache = function(state)
		{
			if (!host)
				return;
			
			if (state == 'validate')
			{
				httpChannel.loadFlags = httpChannel.loadFlags | Ci.nsIRequest.VALIDATE_ALWAYS;
				return;
			}
			
			if (state == 'cache')
			{
				httpChannel.loadFlags = httpChannel.loadFlags | Ci.nsIRequest.LOAD_FROM_CACHE;	// VALIDATE_ONCE_PER_SESSION	VALIDATE_NEVER
				return;
			}

			if (!HTTPUACleaner.HostOptions[hostPP]["Caching"])
				HTTPUACleaner.HostOptions[hostPP]["Caching"] = {urls: []};

			var died = HTTPUACleaner.isHostTimeDied(hostPP, 'Caching', host) !== false;
			if (
				state == "no cache"
				||
				HTTPUACleaner.HostOptions[hostPP]["Caching"].urls.indexOf(httpChannel.URI.spec) < 0
				||
				died
				)
			{
				// https://developer.mozilla.org/en-US/docs/XPCOM_Interface_Reference/nsIRequest
				try
				{
					if (died)
					{
						HTTPUACleaner.HostOptions[hostPP]["Caching"].urls = [];
						HTTPUACleaner.setHostTime(hostPP, 'Caching', host);
					}

					httpChannel.loadFlags = httpChannel.loadFlags
										| Ci.nsIRequest.INHIBIT_PERSISTENT_CACHING
										| Ci.nsIRequest.LOAD_BYPASS_CACHE;
										//| Ci.nsIRequest.VALIDATE_ALWAYS;
/*
					if (context)
					{
						var cacheService = Cc["@mozilla.org/netwerk/cache-storage-service;1"].getService(Ci.nsICacheStorageService);
						var obj = {};
						Cu.import("resource://gre/modules/LoadContextInfo.jsm", obj);
						// http://dxr.mozilla.org/mozilla-central/source/toolkit/modules/LoadContextInfo.jsm
						// http://dxr.mozilla.org/mozilla-central/source/netwerk/cache2/nsICacheStorage.idl
						// http://dxr.mozilla.org/mozilla-central/source/netwerk/cache2/nsICacheStorageService.idl
						var li  = obj.LoadContextInfo.fromLoadContext(context, false);
						var mcs = cacheService.memoryCacheStorage(li);
						var ce  = mcs.openTruncate(httpChannel.URI, context.appId);

						li  = obj.LoadContextInfo.fromLoadContext(context, true);
						mcs = cacheService.memoryCacheStorage(li);
						ce  = mcs.openTruncate(httpChannel.URI, context.appId);
					}
*/
					logMessages.push({name: 'Cache', state: state + ' (NO CACHE)'});
				}
				catch (ex)
				{
					console.error("do not modify loadFlags with exception " + ex);
				}

				if (modify && state != "no cache" && HTTPUACleaner.HostOptions[hostPP]["Caching"].urls.length < 4096)
					HTTPUACleaner.HostOptions[hostPP]["Caching"].urls.push(httpChannel.URI.spec);
			}
			else
			{
				try
				{
					httpChannel.loadFlags = httpChannel.loadFlags | Ci.nsIRequest.INHIBIT_PERSISTENT_CACHING;
					logMessages.push({name: 'Cache', state: state + ' (NO PERSISTENT)'});
				}
				catch (ex)
				{
					console.error("do not modify loadFlags with exception " + ex);
				}
			}
		};
		
		// WWW-Authenticate
		// Authorization	Basic U2Vzc2lvbjo1NTUyMjU3Njk=
		var AuthorizationFunc = function(state)
		{
			var h = null;
			try
			{
				h = httpChannel.getRequestHeader("Authorization");
				HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Authorization header', level: 3, msg: {header: h, tab: obj.taburl}});
			}
			catch (e)
			{}

			httpChannel.setRequestHeader("Authorization", undefined, false);

		};
		
		var hCookies = function(state)
		{
			if (state == 'isolated')
			{
				httpChannel.setRequestHeader("Cookie", 	undefined, false);
				logMessages.push({name: 'Cookie', state: state});
				return;
			}

			if (state == "host" && !host.noTab)
			{
				var httpDomain = HTTPUACleaner.getHostByURI(httpChannel.URI.spec);
				var docDomain  = host;

				if (httpDomain == docDomain)
					return;
			}
			else
			if (state == "domain" && !host.noTab)
			{
				var httpDomain = HTTPUACleaner.getDomainByHost(HTTPUACleaner.getHostByURI(httpChannel.URI.spec));
				var docDomain  = HTTPUACleaner.getDomainByHost(host);

				if (httpDomain == docDomain)
					return;
			}

			httpChannel.setRequestHeader("Cookie", undefined, false);
			httpChannel.loadFlags = httpChannel.loadFlags
										| (1 << 14) /* LOAD_ANONYMOUS */;
			logMessages.push({name: 'Cookie', state: state});
		};

		var AcceptHeader = function(state)
		{
			//"prefill.dynamic.Accept.Safari 5" = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
			//"prefill.dynamic.Accept.MyFox" = "text/html,application/xml;q=0.9,*/*;q=0.8"
			//"prefill.dynamic.Accept.FFox" = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
			//"prefill.dynamic.Accept.SafariChrome" = "application/xml,application/xhtml+xml,text/html;q=0.9, text/plain;q=0.8,image/png,*/*;q=0.5"
			// Opera27	text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8

			if (state == "clean")
			{
				httpChannel.setRequestHeader("Accept", "*/*", false);
				logMessages.push({name: 'Accept header', state: state + ' (*/*)'});
			}
			else
			{
				try
				{
					var ah = httpChannel.getRequestHeader("Accept");
					if (ah.indexOf("text/html") != 0)
					{
						httpChannel.setRequestHeader("Accept", "*/*", false);
						logMessages.push({name: 'Accept header', state: state + ' (*/*)'});
						return;
					}
					
					var AC = '';
					if (ua.indexOf(" OPR/") >= 0)
						AC = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
					else
					if (ua.indexOf("Chrome") >= 0)
						AC = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
					else
					if (ua.indexOf("Safari") >= 0)
						AC = "application/xml,application/xhtml+xml,text/html;q=0.9, text/plain;q=0.8,image/png,*/*;q=0.5";
					else
					if (ua.indexOf("Firefox") >= 0)
						AC = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
					else
					if (ua.indexOf("Opera") >= 0)
						AC = "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/webp, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1";
					else
						AC = "text/html,application/xhtml+xml,application/xml,*/*";
					
					httpChannel.setRequestHeader("Accept", AC, false);
					logMessages.push({name: 'Accept header', state: state + ' (' + AC + ')'});
				}
				catch (ex)
				{
					httpChannel.setRequestHeader("Accept", "", false);
					logMessages.push({name: 'Accept header', state: 'error/' + state});
				}
			}
		};

		var ETag = function(state)
		{
			if (!HTTPUACleaner.HostOptions[hostPP]["ETag"])
			{
				HTTPUACleaner.HostOptions[hostPP]["ETag"] = {  date: new Date(), 'urls': []  };
				HTTPUACleaner.setHostTime(hostPP, 'ETag', host);
			}

			var eq = false;

			var died = HTTPUACleaner.isHostTimeDied(hostPP, 'ETag', host) !== false;
			if
			(
				died
			||
				HTTPUACleaner.HostOptions[hostPP]["ETag"].urls.indexOf(httpChannel.URI.spec) < 0
			)
			{
				if (died)
				{
					HTTPUACleaner.HostOptions[hostPP]["ETag"].urls = [];
					HTTPUACleaner.setHostTime(hostPP, 'ETag', host);
				}

				var found = false;
				
				// tools.ietf.org/html/rfc2616
				var forbidden = ['If-Match', 'If-None-Match', 'If-Range'];
				var visitor = function() {};
				visitor.prototype.visitHeader = function(header, value)
				{
					if (forbidden.indexOf(header) >= 0)
					{
						found = true;
						return;
					}
				};

				httpChannel.visitRequestHeaders(new visitor());

				if (!found)
				{
					httpChannel.loadFlags = httpChannel.loadFlags
								| Ci.nsIRequest.INHIBIT_PERSISTENT_CACHING;

					logMessages.push({name: 'ETag', state: state + ' (no etag)'});
					return;
				}

				httpChannel.loadFlags = httpChannel.loadFlags
								| Ci.nsIRequest.INHIBIT_PERSISTENT_CACHING
								| Ci.nsIRequest.LOAD_BYPASS_CACHE;

				if (modify || HTTPUACleaner.HostOptions[hostPP]["ETag"].urls.length < 4096)
				{
					HTTPUACleaner.HostOptions[hostPP]["ETag"].urls.push(httpChannel.URI.spec);
				}

				logMessages.push({name: 'ETag', state: state + '(NO CACHE)'});
			}
			else
			{
				httpChannel.loadFlags = httpChannel.loadFlags
								| Ci.nsIRequest.INHIBIT_PERSISTENT_CACHING;

				logMessages.push({name: 'ETag', state: state + ' (no actions)'});
			}
		};
		
		if (modify)
		{
			var state		 = HTTPUACleaner.getFunctionState(host, "Authorization");
			if (state != "disabled")
				try
				{
					AuthorizationFunc(state);
				}
				catch (e)
				{}

			var state		 = HTTPUACleaner.getFunctionState(host, 'DNT');
			if (state != "disabled")
				try
				{
					DNT(state);
				}
				catch (e)
				{}


			noCacheRules(f.cachel, f.cachev);

			var state		 = HTTPUACleaner.getFunctionState(host, "Caching");
			if (state != "disabled")
				try
				{
					noCache(state);
				}
				catch (e)
				{}

			var state		 = HTTPUACleaner.getFunctionState(host, "Etag");
			if (state != "disabled")
				try
				{
					ETag(state);
				}
				catch (e)
				{}

			// !modify вызывается выше
			if (HTTPUACleaner.httplog.enabled)
			{
				HTTPUACleaner.LogRequestHeaders(subject, httpChannel, obj.taburl, host, obj, context, modify, isPrivateChannel);
			}

			return;
		}

		// storageFunc - см. выше
		var state		 = HTTPUACleaner.getFunctionState(host, "Referer");
		if (state != "disabled")
			try
			{
				Referer(state);
			}
			catch (e)
			{}

		var state		 = HTTPUACleaner.getFunctionState(host, "CORS");
		if (state != "disabled")
			try
			{
				CORSFunc(state);
			}
			catch (e)
			{}

		var state		 = HTTPUACleaner.getFunctionState(host, 'DNT');
		if (state != "disabled")
			try
			{
				DNT(state);
			}
			catch (e)
			{}

		var state		 = HTTPUACleaner.getFunctionState(host, 'Locale');
		if (state != "disabled")
			try
			{
				Locale(state);
			}
			catch (e)
			{}

		var state		 = HTTPUACleaner.getFunctionState(host, "XForwardedFor");
		if (state != "disabled")
			try
			{
				XForwardedFor(state);
			}
			catch (e)
			{}

		noCacheRules(f.cachel, f.cachev);
		var state		 = HTTPUACleaner.getFunctionState(host, "Caching");
		if (state != "disabled")
			try
			{
				noCache(state);
			}
			catch (e)
			{}

		var state		 = HTTPUACleaner.getFunctionState(host, "Authorization");
		if (state != "disabled")
			try
			{
				AuthorizationFunc(state);
			}
			catch (e)
			{}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "hCookies");
		if (state != "disabled")
			try
			{
				hCookies(state);
			}
			catch (e)
			{}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "AcceptHeader");
		if (state != "disabled")
			try
			{
				AcceptHeader(state);
			}
			catch (e)
			{}
		
		var state		 = HTTPUACleaner.getFunctionState(host, "Etag");
		if (state != "disabled")
			try
			{
				ETag(state);
			}
			catch (e)
			{}

		if (!requestBlocked && logMessages.length > 0)
		{
			var resultMsg = {};			
			for (var a of logMessages)
			{
				resultMsg[a.name] = a.state ? a.state : '';
				if (a.action)
					resultMsg[a.name] += ': ' + a.action;
			}

			if (resultMsg)
			{
				resultMsg['tab'] = obj.taburl;
				HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'HTTP request', level: 1, msg: resultMsg});
			}
		}
	},

	// 3 минуты
	timeToDie: 3 * 60 * 1000,
	
	isDied: function(date, timeToDie)
	{
		if (!date)
			return true;

		if (timeToDie === undefined)
			timeToDie = HTTPUACleaner.timeToDie;

		if (date instanceof Date)
		{
			return Date.now() > new Date(date.getTime() + timeToDie).getTime();
		}
		else
		{
			return Date.now() > new Date(date + timeToDie).getTime();
		}
	},

	isHostTimeDied: function(hostPP, filterName, host)
	{
		var timeToDie = HTTPUACleaner.getFunctionState(host, 'UATI');
		if (timeToDie == "disabled")
		{
			timeToDie = 1000 * 60 * 60 * 24 * 365;
		}
		else
			timeToDie = timeToDie * 60 * 1000;

		if (!timeToDie)
			timeToDie = HTTPUACleaner.timeToDie;

		if
			(
				!HTTPUACleaner.HostOptions[hostPP]['time']
				||
				!HTTPUACleaner.HostOptions[hostPP]['time'].date
				||
				HTTPUACleaner.isDied(  HTTPUACleaner.HostOptions[hostPP]['time'].date, timeToDie  )
			)
		{
			return 1;
		}
		
		if
			(
				!HTTPUACleaner.HostOptions[hostPP][filterName]
				||
				!HTTPUACleaner.HostOptions[hostPP][filterName]['time']
				||
				!HTTPUACleaner.HostOptions[hostPP][filterName]['time'].date
				||
				HTTPUACleaner.isDied(  HTTPUACleaner.HostOptions[hostPP][filterName]['time'].date, timeToDie  )
			)
		{
			return 2;
		}
		
		return false;
	},

	searchPassImg: function(src)
	{
		if (!HTTPUACleaner.imgToPass)
			HTTPUACleaner.imgToPass = [];

		if (HTTPUACleaner.imgToPass.length <= 0)
			return false;

		for (var imgsrc in HTTPUACleaner.imgToPass)
		{
			if (HTTPUACleaner.imgToPass[imgsrc].src == src)
			{
				HTTPUACleaner.imgToPass[imgsrc].founded = true;
				return true;
			}
		}
		
		return false;
	},
	
	deletePassImg: function(src)
	{
		if (!HTTPUACleaner.imgToPass)
			HTTPUACleaner.imgToPass = [];

		if (HTTPUACleaner.imgToPass.length <= 0)
			return false;
		
		var time = Date.now();

		var founded = false;
		for (var i = 0; i < HTTPUACleaner.imgToPass.length; i++)
		{
			var imgsrc = HTTPUACleaner.imgToPass[i];

			if (imgsrc.src == src)
			{
				HTTPUACleaner.imgToPass.splice(i, 1);
				founded = true;
			}
			else
			if (time - imgsrc.time.getTime() > 1*60*1000)	// 1 минута времени
			if (!imgsrc.founded || time - imgsrc.time.getTime() > 60*60*1000) // час времени
				HTTPUACleaner.imgToPass.splice(i, 1);
		}

		return founded;
	},
	
	deleteMetaRefresh: function(tabUri, hostUri)
	{/*
		if (tabUri != hostUri)
			return false;
*/
		// HTTPUACleaner.metaRefreshes.push({  url: document.URL, toURL: a[1], date: Date.now(), edate: 1000 + Number(a[0])  });
		if (!HTTPUACleaner.metaRefreshes || HTTPUACleaner.metaRefreshes.length <= 0)
			return false;

		var time = Date.now();

		var founded = false;
		for (var i = 0; i < HTTPUACleaner.metaRefreshes.length; i++)
		{
			var meta = HTTPUACleaner.metaRefreshes[i];

			if (meta.url == hostUri)
			{
				HTTPUACleaner.metaRefreshes.splice(i, 1);
				founded = meta;
			}
			else
			if (time - meta.date - meta.edate > 0 || time - meta.date > 5*60*1000)
				HTTPUACleaner.metaRefreshes.splice(i, 1);
		}

		return founded;
	},
	
	setHostTime: function(hostPP, filterName, host)
	{
		var a = HTTPUACleaner.isHostTimeDied(hostPP, filterName, host);
		if (a === false)
			return;

		if (!HTTPUACleaner.HostOptions[hostPP])
			HTTPUACleaner.HostOptions[hostPP] = {};

		if (!HTTPUACleaner.HostOptions[hostPP][filterName])
			HTTPUACleaner.HostOptions[hostPP][filterName] = {};

		if (a == 2)
		{
			HTTPUACleaner.HostOptions[hostPP][filterName]['time'] = {date: HTTPUACleaner.HostOptions[hostPP]['time'].date};
		}
		else
		{
			HTTPUACleaner.HostOptions[hostPP]['time'] 			= {date: new Date()};
			HTTPUACleaner.HostOptions[hostPP][filterName]['time'] = {date: new Date()};
		}
	},
	changeCookie_Regex: /-/g,
	changeCookie: function(cookieSetStr, rndStr, hostPP, iCookiesRegime, hCookiesRegime, expired)
	{
		var str = cookieSetStr.split(';');
		var newCookie = str[0];
		
		for (var i = 1; i < str.length; i++)
		{
			var s = str[i].indexOf('=');
			if (s < 0)
			{
				newCookie += '; ' + str[i];
				continue;				
			}

			var key = str[i].substr(0, s).trimLeft();
			var val = str[i].substr(s + 1);
			var kl  = key.toLowerCase();

			if (kl == 'domain')
			{
				if (iCookiesRegime > 0)
					val += '.' + rndStr + '.huac'
			}
			else
			if (kl == 'expires')
			{
				if (expired === true)
					continue;

				// Фильтруем expires, если hCookies:S
				if (iCookiesRegime == 0 && hCookiesRegime == 2)
					continue;

				try
				{
					var dt = new Date(val.replace(HTTPUACleaner.changeCookie_Regex, ' '));
					var subTime = dt.getTime() - Date.now();
					if (Number.isNaN(subTime))
						continue;

					if (subTime > 2 * 60 * 60 * 1000)	// Если больше двух часов, то игнорируем Expires
						continue;
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
					console.error(str[i]);
					console.error(cookiesArray);

					continue;
				}
			}
			else
			if (kl == 'max-age')
			{
				if (expired === true)
					continue
		
				// Фильтруем expires, если hCookies:S
				// т.к. мы устанавливаем сессионные куки
				if (iCookiesRegime == 0 && hCookiesRegime == 2)
					continue;

				try
				{
					var dt = new Date(val);
					var subTime = dt.getTime();
					if (Number.isNaN(subTime))
						continue;

					if (subTime > 2 * 60 * 60 * 1000)	// Если больше двух часов, то игнорируем Max-Age
						continue;
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
					console.error(str[i]);
					console.error(cookiesArray);

					continue;
				}
			}

			newCookie += '; ' + key + '=' + val;
		}

		if (expired === true)
			newCookie += '; EXPIRES=Tuesday, 01-Jan-1970 23:59:59 GMT';

		return newCookie;
	},

	onHttpResponseReceived: function(subject, code)
	{
		if (!HTTPUACleaner.enabled)
			return;

		var obj         = {filled: true};
		var httpChannel = subject.QueryInterface(Ci.nsIHttpChannel);
		var context     = HTTPUACleaner.getHttpContext(subject, httpChannel);
		var host        = HTTPUACleaner.getHostName(httpChannel, subject, false, context, obj);

		var protocol = HTTPUACleaner.getProtocolFromURL(httpChannel.URI.spec);

		if (
			   protocol == "about:"
			|| protocol == "moz-safe-about:"
		)
		{
			return;
		}

		var isPrivateChannel = false;
		try
		{
			try
			{
				var s = subject.QueryInterface(Ci.nsIPrivateBrowsingChannel);
				isPrivateChannel = s.isChannelPrivate;
			}
			catch (e)
			{
				isPrivateChannel = false;
				HTTPUACleaner.logObject(e, true);;
			}

			// Почему-то такое бывает, например, когда фильтр Images грузит с приватного режима картинки
			if (context && !isPrivateChannel && context.usePrivateBrowsing)
			{
				s.setPrivate(true);
				isPrivateChannel = true;
			}
		}
		catch (e)
		{
			isPrivateChannel = false;
			HTTPUACleaner.logObject(e, true);;
		}

		if (HTTPUACleaner.httplog.enabled)
		{
			HTTPUACleaner.LogResponseHeaders(subject, httpChannel, obj.taburl, host, obj, context, code, isPrivateChannel);
		}

		if (HTTPUACleaner.getFunctionState(host, "NoFilters") == 'no filters')
		{
			return;
		}

		
		var cnttype = '_@_';
		try
		{
			cnttype = httpChannel.getResponseHeader("Content-Type");
		}
		catch(e)
		{}

		var onlyHTTPSEnabled = 
					protocol.indexOf('javascript:') != 0
					&&
					(
						HTTPUACleaner.isOnlyHttps(host, HTTPUACleaner.getHostByURI(httpChannel.URI.spec))
					);

		var requestBlocked = false;
		var isHPKP         = {};
		var isOCSP         = HTTPUACleaner.isOCSPResponse(httpChannel, isHPKP, code);

		var TLInfo = null;
		TLSInfo:
		try
		{
			if (!HTTPUACleaner.EstimateTLS && !onlyHTTPSEnabled)
			{
				TLInfo = {f: true};
				break TLSInfo;
			}

			//if (HTTPUACleaner.enabledOptions['OnlyHttps'] != 'disabled')

			var urls = HTTPUACleaner.urls; // require('./getURL');
			var tab = context ? urls.getBrowserForContext(context) : '';

			var uri = context ? urls.fromBrowser(tab) : httpChannel.URI.spec;
			if (obj.taburl)
			{
				// console.error('to taburl ' + uri + ' from ' + uri);
				uri = obj.taburl; //obj.HTTPUACleaner_URI;
			}
			obj.turl = uri;

			try
			{
				if (HTTPUACleaner.EstimateTLS)
				{
					TLInfo = HTTPUACleaner.logger.getTLSInfo(HTTPUACleaner.logger.getSecurityInfo(httpChannel), HTTPUACleaner.certsObject.hosts, isHPKP, HTTPUACleaner.base64, httpChannel, HTTPUACleaner.fxVersion51, HTTPUACleaner.debugOptions.hpkpdigest);
				}

				if (TLInfo == null)
					TLInfo = {f: 0.0, fl: 0.0};
			}
			catch (e)
			{
				TLInfo = {error: e, f: 0.0, fl: 0.0};
				if (HTTPUACleaner.debug)
				{
					console.error("HTTPUACleaner.logger.addToLog has raised exception from getSecurityInfo");
					console.error(uri);
					if (obj)
					console.error(obj.redirectTo);
					if (httpChannel && httpChannel.URI)
					console.error(httpChannel.URI.spec);
					HTTPUACleaner.logObject(e, true);;
				}
			}

			if (TLInfo)
				TLInfo.contentType = cnttype;

			if (
				(
					onlyHTTPSEnabled && HTTPUACleaner.EstimateTLS
				)
				&& TLInfo.f*100.0 <= HTTPUACleaner.minTLSStrong
			)
			{
				// Ср��батывает тот if, что стоит в catch
				if (isOCSP)
				{
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Only HTTPS', level: 4, msg: {'source': 'http response', action: HTTPUACleaner.words['allowed'], type: 'ocsp-response'}});
				}
				else
				{
					httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);

					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Only HTTPS', level: 3, msg: {'source': 'http response', action: HTTPUACleaner.words['blocked'], strong: TLInfo.f}});

					requestBlocked = true;
					TLInfo.contentType += ' (' + HTTPUACleaner.words['blocked'] + ')';
					TLInfo.blocked = true;
				}
			}

			//var metaRefresh = HTTPUACleaner.deleteMetaRefresh(uri, httpChannel.URI.spec);
			//obj.metaRefresh = metaRefresh;
			
			// В некоторых случаях редир��кт происходит через javascript или meta refresh
			// В таком случае, не заморачиваясь с массивами, из предположения, что грузится один документ
			// Выясняем, что этот документ новый, если url нового документа совпадает с текущим url
			obj.topdocument = false;
			if (HTTPUACleaner.lastDocument)
			{
				// Даём минуту на подгрузку
				if (Date.now() - HTTPUACleaner.lastDocument.time < 60*1000)
				{
					if (HTTPUACleaner.lastDocument.url == httpChannel.URI.spec && HTTPUACleaner.lastDocument.taburl == uri)
					{
						obj.topdocument = true;
					}
				}
				else
					HTTPUACleaner.lastDocument = null;
			}

			TLInfo.remoteAddress = '';
			var remoteAddress = '';
			try
			{
				var HCI = subject.QueryInterface(Ci.nsIHttpChannelInternal);
				TLInfo.remoteAddress = HTTPUACleaner.words['cached'];
				TLInfo.remoteAddress = HCI.remoteAddress + ':' + HCI.remotePort;
				remoteAddress = HCI.remoteAddress + ':' + HCI.remotePort;
			}
			catch (e)
			{}

			HTTPUACleaner.logger.addToLog(uri, uri, obj, HTTPUACleaner.getHostByURI(uri), httpChannel.URI.spec, TLInfo);

			if (requestBlocked)
				return;
		}
		catch (e)
		{
			if (HTTPUACleaner.debug && !isOCSP)
			{
				console.error("HTTPUACleaner.logger.addToLog has raised exception");
				console.error(uri);
				console.error(obj.redirectTo);
				console.error(httpChannel.URI.spec);
				HTTPUACleaner.logObject(e, true);;
			}
			
			if (isOCSP)
			{
				HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Only HTTPS', level: 4, msg: {'source': 'http response', action: HTTPUACleaner.words['allowed'], type: 'ocsp-response', error: e.message}});
			}
		}

		if (onlyHTTPSEnabled)
		try
		{
			var spec = httpChannel.getResponseHeader("Location");
			if (spec.substr(0, 5) == "http:")
			{
				httpChannel.setResponseHeader("Location", 'https' + spec.substr(4), false);

				HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Only HTTPS', level: 2, msg: {'source': 'http response', action: 'redirect modified'}});
			}
		}
		catch (e)
		{}

		let f = {iCookies: 0};

		// Работа правил вкладки Side
		if (HTTPUACleaner.loggerSide.enabled)
		try
		{
			TLInfo.origin       = null;
			TLInfo.isOCSP       = isOCSP;
			TLInfo.minTLSStrong = HTTPUACleaner.minTLSStrong;
			TLInfo.haveContext  = !!context;

			TLInfo.isPrivate = isPrivateChannel;

/*
			try
			{
				httpChannel.setResponseHeader('Connection', 'keep-alive', false);
				httpChannel.setResponseHeader('Keep-Alive', 'timeout=1', false);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}
*/

			// obj.taburl obj.redirectTo
			obj.turl = obj.taburl ? obj.taburl : httpChannel.URI.spec;
			TLInfo.hstatus = httpChannel.responseStatus;
			f = HTTPUACleaner.sdb.checkRules.response.bind(HTTPUACleaner.sdb)(httpChannel.URI.spec, obj, {rtype: cnttype ? cnttype : '_@_', ftype: undefined}, TLInfo, 'http response');

			if (f.cookie === true)
				httpChannel.setResponseHeader('Set-Cookie', undefined, false);

			var blockedToHttps = false;
			if (f.cancel === true || f.toHttps === true)
			{
				
				if (f.toHttps === true)
				if (httpChannel.URI.scheme != 'https' && httpChannel.URI.scheme != 'wss')
				{
					httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Only HTTPS', level: 3, msg: {'source': 'http response', action: HTTPUACleaner.words['blocked']}});
					blockedToHttps = true;
				}

				if (!blockedToHttps && f.cancel === true)
				{
					if (TLInfo.contentType.indexOf("image/") == 0 && HTTPUACleaner.searchPassImg(httpChannel.URI.spec))
					{
						// Отказываемся от блокирования подгрузки изображения, подгружаемого по клику
						f.log.msg['image allowed'] = 'allowed by load image by click';
						f.cancel === false;
					}
					else
						httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
				}

				// Здесь нельзя ставить return, т.к. логирование происходит позже
			}

			f.log.msg.ct = cnttype ? cnttype : '_@_';

			// Здесь мы всегда логируем, хотя бы для автоматизации по клавише F9, ну и на всякий пожарный случай
			// if (f.executed === true || f.log.level != 1)
			HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, f.log);

			if (f.cancel === true || blockedToHttps === true)
				return;
			

			try
			{			
				var channelHost   = HTTPUACleaner.getHostByURI(httpChannel.URI.spec);
				var channelDomain = HTTPUACleaner.getDomainByHost(channelHost);

				if (f.certs == 1 || f.certs >= 3)
				if (
					(httpChannel.URI.scheme == 'https' || httpChannel.URI.scheme == 'wss')
					&&
					HTTPUACleaner.certsObject.isUnknownCertificate(f.certs, TLInfo, host, channelHost, channelDomain, remoteAddress, httpChannel, obj.taburl)
					)
				{
					if (HTTPUACleaner.certsObject.unknownCerfiticateBlock(f.certs, TLInfo, host, HTTPUACleaner.getHostByURI(httpChannel.URI.spec), HTTPUACleaner.getDomainByHost(httpChannel.URI.host), remoteAddress, httpChannel, obj.taburl))
						return;
				}

				if (HTTPUACleaner.certsObject.hosts)
				if (!TLInfo.isPrivate || HTTPUACleaner.certsObject.hostsPrivate)
				{
					HTTPUACleaner.certsObject.log(TLInfo, host, channelHost, channelDomain, remoteAddress, isHPKP);
				}
			}
			catch (e)
			{
				console.error('HUAC ERROR: certsObject.log or certsObject.isUnknownCertificate');
				HTTPUACleaner.logObject(e, true);;
			}
		}
		catch (e)
		{
			console.error("FATAL ERROR");
			console.error("HTTPUACleaner.sdb.checkRules.response has raised exception");
			console.error(uri);
			console.error(obj.redirectTo);
			console.error(httpChannel.URI.spec);
			HTTPUACleaner.logObject(e, true);;
		}


		var hostPP = (TLInfo.isPrivate ? ':' : '') + host;

		var domain_for_cookies = HTTPUACleaner.getDomainByHost(host);
		if (TLInfo.isPrivate)
			domain_for_cookies = ':' + domain_for_cookies;
		
		HTTPUACleaner.createHostOptions(hostPP);

		
		
		
		// Заголовок Content-Security-Policy ограничивает возможности по работе с кодом для фильтра TimeZone
		// Чтобы этого не было, вставляем хеши кода, который будем исполнять во вставленных тегах script
		if (HTTPUACleaner.getFunctionState(host, "TimeZone") != 'disabled')
		{
			let cspHeaderIsSet = false;
			var valCSP = '';
			try
			{
				valCSP = httpChannel.getResponseHeader('Content-Security-Policy');
				cspHeaderIsSet = true;
			}
			catch (e)
			{}

			HTTPUACleaner.HostOptions[hostPP]['Content-Security-Policy'] = {};
			var cspHO = HTTPUACleaner.HostOptions[hostPP]['Content-Security-Policy'];

			if (cspHeaderIsSet)
			try
			{
				cspHO.yes = true;

				var vl = valCSP.split(';');
				valCSP = '';

				let defaultPolicy = false;
				let scriptCSPSetted = false;
				let scriptCSPUnsafeInline = false;
				let scriptCSPNonceOrHash = false;

				let newScriptSrc = "'sha256-wtbhxSxjJXXtc22eU2WcxEQWuhRx3a6GGYfZSEzYEO0='";
				for (var i = 0; i < vl.length; i++)
				{
					if (vl[i].trim().startsWith('script-src '))
					{
						// CSP блокирует директивы 'unsafe-inline' ('unsafe-eval', кажется, нет) если вставляем директивы hash или nonce.
						var ax = vl[i].split(' ');

						for (var j = 0; j < ax.length; j++)
						{
							if (ax[j].startsWith("'nonce-") && ax.indexOf("'hash-"))
							{
								scriptCSPNonceOrHash = true;
								break;
							}
						}

						if (ax.indexOf("'unsafe-eval'") >= 0)
						{
								cspHO.unsafeEval = true;
								scriptCSPUnsafeInline = true;
						}

						if (ax.indexOf("'unsafe-inline'") >= 0)
						{
							cspHO.unsafeInline = true;
							scriptCSPUnsafeInline = true;
						}
						
						if (ax.indexOf("'none'") >= 0)
							cspHO.noneDirective = true;

						var vlscr = vl[i].substr(vl[i].indexOf('script-src') + 'script-src'.length);

						if (cspHO.noneDirective)
							vlscr = 'script-src ' + newScriptSrc;
						else
							vlscr = 'script-src ' + newScriptSrc + vlscr;


						valCSP += vlscr + ';';
						scriptCSPSetted = true;
					}
					else
					{
						if (vl[i].trim().startsWith('default-src '))
						{
							defaultPolicy = vl[i].substr(vl[i].indexOf('default-src') + 'default-src'.length);
							var dfpSplitted = defaultPolicy.split(' ');

							if (dfpSplitted.indexOf("'none'") >= 0)
								defaultPolicy = '';
						}

						valCSP += vl[i] + ';';
					}
				}

				// Если директива script-src не задана, то нужно добавить политику по умолчанию, если она не 'none'
				if (!scriptCSPSetted)
				{
					if (defaultPolicy)
						valCSP += 'script-src ' + newScriptSrc + defaultPolicy;
					else
						valCSP += 'script-src ' + newScriptSrc;
				}

				// scriptCSPUnsafeInline означает, что указаны директивы unsafe-inline или unsafe-eval,
				// т.е. скрипт можно будет и так вставить, без изменения CSP
				// scriptCSPNonceOrHash означает, что директива unsafe-inline будет проигнорирована,
				// т.к. установлена хотя бы одна директива nonce или hash
				// cspHO.noneDirective означает, что установлена директива 'none', а значит политику script-src нужно сбрасывать в любом случае
				if (!scriptCSPUnsafeInline || scriptCSPNonceOrHash || cspHO.noneDirective)
					httpChannel.setResponseHeader('Content-Security-Policy', valCSP, false);
			}
			catch (e)
			{
				console.error(httpChannel.URI.spec);
				console.error(e);
			}
			else
			{
				cspHO.yes = false;
			}
		}

		
		
		var iCookiesSet = function(regime, hRegime)
		{
			HTTPUACleaner.generateNewiCookieRandomStrIfNotSet(hostPP, host, regime, domain_for_cookies);

			var visitor = function() {};
			let found   = false;
			let cookiesArray    = [];
			let cookiesArrayMod = [];
			var ServerDate = null;
			visitor.prototype.visitHeader = function(header, value)
			{
				if (header == 'Date')
				{
					ServerDate = value;
					return;
				}

				if (header != 'Set-Cookie')
					return;

				var ck = value.split("\n");
				if (ck.length <= 0)
					return;
				
				found = true;
				for (var c of ck)
				{
					//this.visitHeader(header, c);
					cookiesArray.push(c);
				}

				return;
			};

			var visitora = new visitor();
			visitora.visitHeader = visitora.visitHeader.bind(visitora);
			httpChannel.visitResponseHeaders(visitora);

			httpChannel.setResponseHeader('Set-Cookie', undefined, false);			
			if (found)
			{
				var setCookieFromHttp = function(hostPP)
				{
					var rndStr = HTTPUACleaner.generateCookieRndStr(TLInfo.isPrivate, TLInfo.haveContext, regime, hostPP, domain_for_cookies);

					var PC  = null;
					var PCC = null
					if (httpChannel.URI.spec.substr(0, 6) == 'https:' || httpChannel.URI.spec.substr(0, 6) == 'wss:')
					{
						PC  = HTTPUACleaner.privateChannelS;
						PCC = HTTPUACleaner.ChannelS;
					}
					else
					{
						PC  = HTTPUACleaner.privateChannelN;
						PCC = HTTPUACleaner.ChannelN;
					}

					for (var ck of cookiesArray)
					{
						cookiesArrayMod.push
						(
							HTTPUACleaner.changeCookie(ck, rndStr, hostPP, regime, hRegime)
						);
					}

					var cs = HTTPUACleaner.cs;
					for (var ck of cookiesArrayMod)
					{
						var newUri = httpChannel.URI.clone();
						if (regime > 0)
							newUri.host = httpChannel.URI.host + '.' + rndStr + '.huac';

						//cs.setCookieString(newUri, null, ck, /*httpChannel*/PC);
						cs.setCookieStringFromHttp(newUri, null, null, ck, ServerDate, (TLInfo.isPrivate || regime > 0) ? PC : PCC);
						/*console.error(newUri.spec);
						console.error((TLInfo.isPrivate || regime > 0));
						console.error(ck);*/
					}
				};

				setCookieFromHttp(hostPP);

				HTTPUACleaner.HostOptions[hostPP]
			}
		};

		if (f.iCookies > 0 || f.cookie == 2)
		{
			try
			{
				// Вставка куков для целей отладки
				// httpChannel.setResponseHeader('Set-Cookie', 'huac=123456789; HTTPONLY', true);

				// Фильтр iCookies
				iCookiesSet(f.iCookies, f.cookie);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
				httpChannel.setResponseHeader('Set-Cookie', undefined, false);
			}
		}

		// Работа с перенаправлением на картинки, которые хочет загрузить пользователь
		try
		{
			var spec = httpChannel.getResponseHeader("Location");
			if (HTTPUACleaner.searchPassImg(httpChannel.URI.spec))
			{
				try
				{
					HTTPUACleaner.deletePassImg(httpChannel.URI.spec);
				}
				catch (e)
				{}

				HTTPUACleaner.imgToPass.push({src: spec, time: new Date()});
			}
		}
		catch (e)
		{}

		var allowImage = function(type)
		{
			var state = HTTPUACleaner.getFunctionState(host, "Images");

			if (type.indexOf("image/") == 0)
			{
				if (state == "disabled" || HTTPUACleaner.searchPassImg(httpChannel.URI.spec))
				{
					// Ес��и приходит ответ, что результат не изменился, то потом ещё прийд��т ответ, что изображени�� взято и�� кеша
					if (httpChannel.responseStatus != 304)
					{
						HTTPUACleaner.deletePassImg(httpChannel.URI.spec);
					}
					
					if (state != 'disabled')
						HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Image', level: 0, msg: {'source': 'http response', 'action': HTTPUACleaner.words['allowed']}});

					return 0;
				}
				else
				{
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Image', level: 7, msg: {'source': 'http response', type: type}});
					
					// Нам всё равно нужно удалить url из списка
					HTTPUACleaner.deletePassImg(httpChannel.URI.spec);

					return -1;
				}
			}
			
			// И здесь, когда это не изображение, нам тоже нужно удалить url из списка
			HTTPUACleaner.deletePassImg(httpChannel.URI.spec);
			
			return 1;
		};

		// Разрешает загрузку аудио и видео
		var allowAudio = function(type)
		{
			var state = HTTPUACleaner.getFunctionState(host, "Audio");

			if (type.indexOf("audio/") == 0 || type.indexOf("video/") == 0)
			{
				if (state == "disabled")
					return 0;
				else
				{
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Media', level: 7, msg: {'source': 'http response', type: type}});
					return -1;
				}
			}

			return 1;
		};
	
		var cancelRequest = function(type)
		{
			if (HTTPUACleaner.getDomainByHost(host) == "mozilla.net" || HTTPUACleaner.getDomainByHost(host) == "mozilla.org" || HTTPUACleaner.getDomainByHost(host) == "mozilla.com")
			{
				if (type)
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Allow Mozilla', level: 0, msg: {'source': 'http response', type: type}});
				else
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Allow Mozilla', level: 0, msg: {'source': 'http response'}});
				
				return false;
			}
			else
			{
				httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
				return true;
			}
		}

		var stateoh	 = HTTPUACleaner.getFunctionState(host, "OnlyHtml");
		endTypes:
		try
		{
			// getResponseHeader на кешированном запросе с результатом HTTP/1.1 304 Not Modified может ничего не дать
			var type = httpChannel.getResponseHeader("Content-Type");
			var scTypeIndex = type.indexOf(";");
			if (scTypeIndex > 0)
			{
				type = type.substring(0, scTypeIndex);
				//console.error("TYPE: " + type);
			}

			var isContentType = function (type, contentTypes)
			{
				var tp = type.toLowerCase();
				for (var index in contentTypes)
				{
					if (contentTypes[index] == tp)
						return true;
					if (tp.indexOf(contentTypes[index] + ";") == 0)
						return true;
				}
				
				return false;
			}

			endType:
			{
				var isImage = allowImage(type);

				if (isImage == -1)
				{
					// httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
					cancelRequest(type);
					return;
				}

				var isAudio = allowAudio(type);
				if (isAudio == -1)
				{
					// httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
					cancelRequest(type);
					return;
				}

				if (stateoh != "disabled" && isImage != 0 && isAudio != 0 /* && (httpChannel.requestMethod != "POST" && httpChannel.requestMethod != "PUT")*/)
				{
					// // application/json; application/ocsp-response; text/css; application/x-javascript; text/javascript; image/...; audio/...;
					// X-Content-Type-Options: nosniff
					// x-dns-prefetch-control: off
					error:
					{	// plain/text - бывает и такое
						if (isContentType(type, ["text/plain", "text/html", "application/ocsp-response", "plain/text", 'application/rdf+xml', 'application/rdf']))
							break endType;

						if (stateoh == "html")
						{
							HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Non html', level: 6, msg: {'source': 'http response', type: type}});
							
							break error;
						}

						if (isContentType(type, ["text/css"]))
							break endType;

						// css/xml отображается как css и не разрешает xml
						if (stateoh == "css/xml")
						{
							HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Non html/css', level: 6, msg: {'source': 'http response', type: type}});
							
							break error;
						}

						// Верные типы скриптов
						// "application/x-javascript", "application/javascript", 'application/ecmascript', 'text/ecmascript', "text/javascript"
						// "application/json", "text/json"
						if (isContentType(type, ["application/x-javascript", "application/javascript", 'application/ecmascript', 'text/ecmascript', "text/javascript", "text/json", "application/json", "application/js", "text/js", "text/x-javascript", "application/x-json", "text/x-json", "application/json-rpc", "application/xhtml+xml", "application/xml", "text/xml", "application/xml", "text/x-cross-domain-policy"]))
							break endType;
						
						if (stateoh == "js")
						{
							HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'Non html/css/xml/js', level: 6, msg: {'source': 'http response', type: type}});
							
							break error;
						}
						
						break endType;
					}

					if (cancelRequest(type))
					try
					{
/*
						var notifications = require("sdk/notifications");
						notifications.notify
						({
							title: 		HTTPUACleaner['sdk/l10n'].get("Blocked restricted type") + " " + type,
							text: 		HTTPUACleaner.notificationNumber + ": " + host  + "\r\n" + httpChannel.URI.spec,
							iconURL: 	HTTPUACleaner['sdk/self'].data.url("HTTPUACleaner.png"),
							data: 		"" + HTTPUACleaner.notificationNumber,
							onClick: 	function() {}
						});*/

/*
						var nbox = HTTPUACleaner['sdk/window/utils'].getMostRecentBrowserWindow().gBrowser.getNotificationBox();
						try
						{
							//nbox = require("sdk/tabs/utils").getTabBrowserForTab(tab).getNotificationBox();
							//.getTabForContentWindow(document.defaultView);
							var context  = HTTPUACleaner.getHttpContext(subject, httpChannel);
							var I = HTTPUACleaner['sdk/tabs/utils'];
							nbox = HTTPUACleaner.HC.getBrowserForContext(context).ownerDocument.getNotificationBox();
						}
						catch (e)
						{
						}

						nbox.appendNotification
						(
							HTTPUACleaner['sdk/l10n'].get("Blocked restricted type") + " " + type + " for " + host  + " / " + httpChannel.URI.spec,
							"HTTPUACleaner",
							HTTPUACleaner['sdk/self'].data.url("HTTPUACleaner.png"),
							"PRIORITY_WARNING_HIGH",
							null
						);*/
					}
					catch (e)
					{
						console.error("HUAC. Notificatin error");
						HTTPUACleaner.logObject(e, true);;
					}

					// httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);	// NS_ERROR_ABORT (0x80004004) https://developer.mozilla.org/en-US/docs/Table_Of_Errors
				}
			}

		}
		catch (e)
		{
			// 204 - no Content
			if (httpChannel.responseStatus == 204)
			{
				HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'No content (HTTP 204)', level: 0, msg: {'source': 'http response'}});
			}
			else
			// если 304 - то, добро пожаловать. Считаем, что ничего плохого от загрузки не будет.
			// если < 200 - это возвращаемые значения на WebSocket;
			if ((httpChannel.responseStatus > 399 || httpChannel.responseStatus < 300) && httpChannel.responseStatus >= 200)
			{
				if (stateoh != "disabled")
				{
					// у некоторых людей страницы защиты от роботов яндекса не грузятся из-за этого
					// Проверка работы - http://www.pravda.com.ua/
					// httpChannel.cancel(Cr.NS_ERROR_NOT_IMPLEMENTED);
					if (cancelRequest())
					{
						HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'No Content-Type', level: 2, msg: {'source': 'http response', 'action': HTTPUACleaner.words['blocked']}});
/*
						var notifications = require("sdk/notifications");
						notifications.notify
						({
							title: 		"HTTP UserAgent cleaner block no content-type url",
							text: 		HTTPUACleaner['sdk/l10n'].get("URL not have Content-Type - blocked\r\n") + host  + "\r\n" + httpChannel.URI.spec
											+ "\r\n\r\n" + require("sdk/l10n").get("Set 'Only HTML' filter for host to disabled state"),
							iconURL: 	HTTPUACleaner['sdk/self'].data.url("HTTPUACleaner.png")
						});*/
					}
				}
				else
					HTTPUACleaner.loggerB.addToLog(obj.taburl, false, httpChannel.URI.spec, {type: 'No Content-Type', level: 0, msg: {'source': 'http response', 'action': HTTPUACleaner.words['allowed']}});
/*
				if (HTTPUACleaner.debug)
				{
					console.error("UA HTTP Cleaner. Types filter error, HTTP " + httpChannel.responseStatus + ":" + httpChannel.URI.host + httpChannel.URI.path);
					HTTPUACleaner.logObject(e, true);;
				}*/
			}
		}

		var noCache = function(state)
		{
			if (!host)
				return;
		};

		var hCookies = function(state)
		{
			if (!host)
				return;

			if (state == 'isolated')
			{
				httpChannel.setResponseHeader('Set-Cookie', 	undefined, false);
				return;
			}

			if (state == "host" && !host.noTab)
			{
				var httpDomain = HTTPUACleaner.getHostByURI(httpChannel.URI.spec);
				var docDomain  = host;

				if (httpDomain == docDomain)
					return;
			}
			else
			if (state == "domain" && !host.noTab)
			{
				var httpDomain = HTTPUACleaner.getDomainByHost(HTTPUACleaner.getHostByURI(httpChannel.URI.spec));
				var docDomain  = HTTPUACleaner.getDomainByHost(host);

				if (httpDomain == docDomain)
					return;
			}

			httpChannel.setResponseHeader("Set-Cookie", 	undefined, false);
		};

		var HPKPFunc = function(state)
		{
			// max-age - в секундах
			// max-age=1296000; pin-sha256="r/mIkG3eEpVdm+u/ko/cwxzOMo1bk4TyHIlByibiA5E="; pin-sha256="WoiWRyIOVNa9ihaBciRSC7XHjliYS9VwUGOIud4PB18=";
			// max-age=600; includeSubDomains; pin-sha256="WoiWRyIOVNa9ihaBciRSC7XHjliYS9VwUGOIud4PB18="; pin-sha256="5C8kvU039KouVrl52D0eZSGf4Onjo4Khs8tmyTlV3nU="; pin-sha256="5C8kvU039KouVrl52D0eZSGf4Onjo4Khs8tmyTlV3nU="; pin-sha256="lCppFqbkrlJ3EcVFAkeip0+44VaoJUymbnOaEUk7tEU="; pin-sha256="TUDnr0MEoJ3of7+YliBMBVFB4/gJsv5zO7IxD9+YoWI="; pin-sha256="x4QzPSC810K5/cMjb05Qm4k3Bw5zBn4lTdO/nEW/Td4=";
			// TLInfo.sCerts[i].sha256SubjectPublicKeyInfoDigest

			// Если полагается сбрасывать информацию
			if (state == 2)
			{
				// Не сбрасываем новую информацию, скорее всего, мы не хотим её сбрасывать, т.к. она корректная
				// HTTPUACleaner.certsObject.clearHostHPKP(httpChannel.URI.host);
				return;
			}

			// Фильтр работает некорректно в приватном режиме
			if (TLInfo.isPrivate)
				return;

			if ((!!isHPKP.HPKP && isHPKP.HPKP >= HTTPUACleaner.certsObject.HPKP_minMaxAge) && state < 6)
			{
				return;
			}

			// Не реагируем на кешированные запросы и валидированные запросы, взятые из кеша
			if (code == 'ImLYE2mChLYJvHcV9EvuPYwV1B9' || code == '2BROHTPcf2RtJzXuCyrwhHKZgT7' || httpChannel.responseStatus >= 300 && httpChannel.responseStatus <= 304)
			{
				return;
			}

			if (!httpChannel.URI.spec.startsWith('https') && !httpChannel.URI.spec.startsWith('wss'))
				return;

			if (!TLInfo.sCerts || !TLInfo.sCerts.length)
				return;

/*
			// Я не знаю, старые это сертификаты или новые. Лучше я обновлю, если нет HPKP-заголовка (Public-Key-Pins)
			if (HTTPUACleaner.HPKPService.isSecureURI(Ci.nsISiteSecurityService.HEADER_HPKP, httpChannel.URI, 0))
				return;
*/

			var cState = state - 3;
			if (state > 6)
				cState -= 3;

			var cert   = null;
			if (cState < 2 && TLInfo.sCerts.length > cState)
			{
				if (TLInfo.sCerts.length > cState)
					cert = TLInfo.sCerts[cState];
				else
					cert = TLInfo.sCerts[TLInfo.sCerts.length - 1];
			}
			else
				cert = TLInfo.sCerts[TLInfo.sCerts.length - 1];

			var maxAge = (cert.notAfter/1000 - Date.now())/1000;
			var minma = HTTPUACleaner.certsObject.HPKP_minMaxAgeReplace*24*3600;

			if (!!isHPKP.HPKP && isHPKP['max-age'] && maxAge < isHPKP['max-age'])
				maxAge = isHPKP['max-age'];

			if (maxAge <= minma)
			{
				maxAge = minma;
				if (cState === 0)
				{
					cState = 1;
					cert = TLInfo.sCerts[cState];
				}
				else
				{
					cState = 2;
					cert = TLInfo.sCerts[TLInfo.sCerts.length - 1];
				}
			}

			if (maxAge > 365.25*24*3600 && maxAge > (minma << 1))
			{
				maxAge = Math.max(365.25*24*3600, minma << 1) + Math.random() * 24 * 3600;
			}

			var urls = HTTPUACleaner.certsObject.options && HTTPUACleaner.certsObject.options.urls ? HTTPUACleaner.certsObject.options.urls.urls : null;
			var certsToAdd = [cert.sha256SubjectPublicKeyInfoDigest];
			var uriHost = HTTPUACleaner.getHostByURI(httpChannel.URI.spec);
			if (urls && urls[uriHost])
			{
				for (var ip in urls[uriHost].IPs)
				{
					var IP = urls[uriHost].IPs[ip];
					for (var sha in IP.noRoot)
					{
						if (!IP.noRoot[sha].crt || !IP.noRoot[sha].crt.sha256SubjectPublicKeyInfoDigest)
							continue;

						var h = TLInfo.sCerts.length;
						if (IP.noRoot[sha].i == h - cState - 1)
						{
							var val = IP.noRoot[sha].crt.sha256SubjectPublicKeyInfoDigest;
							if (certsToAdd.indexOf(val) < 0)
								certsToAdd.push(val);
						}
					}
				}
			}

			// Слияние заголовков hpkp и принудительного hpkp дополнения
			if (!!isHPKP.HPKP && isHPKP['Public-Key-Pins'] && state < 6)
			{
				var hpkpHeader = isHPKP['Public-Key-Pins'].split(';');
				for (var h of hpkpHeader)
				{
					try
					{
						var eIndex = h.indexOf('=');
						if (eIndex < 0)	// includeSubdomains
							continue;

						var name = h.substring(0, eIndex).trim();
						var val  = h.substring(eIndex+1).trim()
						val      = val.substring(1, val.length - 1);	// Убираем первый и последний символ - это кавычки

						if (name != 'pin-sha256')	// max-age
							continue;

						var index = -1;
						if (cState < 2)
						for (var crtI in TLInfo.sCerts)
						{
							// Пропускаем серверные и другие сертификаты, соответствующие статусу фильтра
							if (crtI <= cState)
								continue;

							var crt = TLInfo.sCerts[crtI];
							if (val == crt.sha256SubjectPublicKeyInfoDigest)
							{
								index = crtI;
								break;
							}
						}

						if (index !== -1)
							continue;

						if (certsToAdd.indexOf(val) < 0)
						{
							certsToAdd.push(val);
						}
					}
					catch (e)
					{
						console.error('HUAC minor error');
						HTTPUACleaner.logObject(e, true);;
					}
				}
			}

			// https://dxr.mozilla.org/mozilla-central/source/security/manager/ssl/nsISiteSecurityService.idl
			HTTPUACleaner.HPKPService.setKeyPins(httpChannel.URI.host, false, Math.floor(maxAge), certsToAdd.length, certsToAdd);
			// HTTPUACleaner.HPKPService.setKeyPins(httpChannel.URI.host, false, Math.floor(maxAge), 1, ['pfR2cczd1CMXmhkebDR9yibNxxO3hWqwWg9h++Dcl8U=']);

			try
			{
				httpChannel.setResponseHeader('Public-Key-Pins', undefined, false);
			}
			catch (e)
			{}

		};
		
		var ETag = function(state)
		{
			if (!host)
				return;
		};

		var statec		 = HTTPUACleaner.getFunctionState(host, "Caching");
		if (statec != "disabled")
			try
			{
				noCache(statec);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}

		var state		 = HTTPUACleaner.getFunctionState(host, "hCookies");
		if (state != "disabled")
			try
			{
				hCookies(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}

		var state		 = HTTPUACleaner.getFunctionState(host, "Etag");
		if (state != "disabled")
			try
			{
				ETag(state);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}

		if (f.CertsHPKP > 1)
			HPKPFunc(f.CertsHPKP);

		HTTPUACleaner.TrustLevelToBadge();
	},
	
	// ----- end
	
	onHttpResponseCached: function(subject, topic)
	{
		if (!HTTPUACleaner.enabled)
			return;

		var httpChannel = subject.QueryInterface(Ci.nsIHttpChannel);
		var context     = HTTPUACleaner.getHttpContext(subject, httpChannel, true);
		var host        = HTTPUACleaner.getHostName(httpChannel, subject, true, context, {onHttpResponseCached: true});

		var protocol = HTTPUACleaner.getProtocolFromURL(httpChannel.URI.spec);

		if (
			   protocol == "about:"
			|| protocol == "moz-safe-about:"
		)
		{
			HTTPUACleaner.getHostName(httpChannel, subject, false, context, {onHttpResponseCached: true});	// для очистки
			return;
		}
/*
		var state		 = HTTPUACleaner.getFunctionState(host, "Caching");
		if (state != "disabled")
		{
			httpChannel.cancel(Cr.NS_ERROR_CACHE_READ_ACCESS_DENIED);
			HTTPUACleaner.getHostName(httpChannel, subject, false, context, {onHttpResponseCached: true});	// для очистки
			return;
		}
*/

		if (topic == 'http-on-examine-merged-response')
			HTTPUACleaner.onHttpResponseReceived(subject, "ImLYE2mChLYJvHcV9EvuPYwV1B9");
		else
			HTTPUACleaner.onHttpResponseReceived(subject, "2BROHTPcf2RtJzXuCyrwhHKZgT7");
	},
	
	getFunctionState: function(host, optionsName, nonGlobal)
	{
		if (!nonGlobal)
			nonGlobal = false;
		
		var domain 		 = HTTPUACleaner.getDomainByHost(host);
		if (domain == host)
			host = "." + host;

		var currentState = HTTPUACleaner.enabledOptions[optionsName];
		var domainState  = HTTPUACleaner.enabledOptions[optionsName + "Domain"][domain];
		var hostState    = HTTPUACleaner.enabledOptions[optionsName + "Domain"][host];

		var state        = HTTPUACleaner.getPriorityState(currentState, domainState, hostState, optionsName, domain, host, nonGlobal);

		return state;
	},
	
	createHostOptions: function(hostPP, state)
	{
		if (!HTTPUACleaner.HostOptions[hostPP])
			HTTPUACleaner.HostOptions[hostPP] = {state: state, CacheControl: {}, navigatorLastTime: 0};
	},
	
	getEtalonNavigatorObject: function(state, hostPP, statem, host)
	{
		var opts = HTTPUACleaner.HostOptions;
		var navigator = {};

		if (HTTPUACleaner.HostOptions[hostPP] && HTTPUACleaner.HostOptions[hostPP].state != state)
			HTTPUACleaner.destroyHostOptions(host, 'getEtalonNavigatorObject ' + state + '/' + HTTPUACleaner.HostOptions[hostPP].state);
		
		/*
		if (!HTTPUACleaner.HostOptions[hostPP])
			HTTPUACleaner.HostOptions[hostPP] = {state: state, CacheControl: {}, navigatorLastTime: 0};*/
		HTTPUACleaner.createHostOptions(hostPP, state);

		if (state == "enabled" || state == "raise error")
		{
			HTTPUACleaner.HostOptions[hostPP].navigator = navigator;
		}
		else
		{
			if (
				   !HTTPUACleaner.HostOptions[hostPP].navigator
				|| statem != "disabled"
				|| HTTPUACleaner.isHostTimeDied(hostPP, 'UA', host) !== false
				)
			{
				navigator = HTTPUACleaner.generateRandomUA(state == "low random", statem);
				HTTPUACleaner.HostOptions[hostPP].navigator = navigator;
				HTTPUACleaner.setHostTime(hostPP, 'UA', host);
			}
			else
			{
				navigator = HTTPUACleaner.HostOptions[hostPP].navigator;
			}
		}

		return navigator;
	},
/*
	BreakageFace: function(host)
	{
		var CLD = 0;
		var c   = 3000 + HTTPUACleaner.getRandomInt(12, 512 + (Date.now() % 512));
		var hnp = host.replace(/\./g, "");
		var hl  = hnp.length;

		var A   = new Array(c + hl);
		for (var i = 0; i < hl; i++)
		{
			CLD += String.charCodeAt(hnp[i]);
			//A.push(host[i]);
			A[i] = hnp[i];
		}

		for (var  i = 0; i < c; i++)
			//A.push(  String.fromCharCode(HTTPUACleaner.getRandomIntShift(32, 127, CLD))  );
		{
			//A[i + hl] = String.fromCharCode(HTTPUACleaner.getRandomIntShift(32, 127, CLD));
			if (i % 7 == 0)
				A[i + hl] = String.fromCharCode(HTTPUACleaner.getRandomIntShift(48, 57+1, CLD));
			else
			if (i % 7 < 4)
				A[i + hl] = String.fromCharCode(HTTPUACleaner.getRandomIntShift(97, 122+1, CLD));
			else
				A[i + hl] = String.fromCharCode(HTTPUACleaner.getRandomIntShift(65, 90+1, CLD));
		}

		HTTPUACleaner.shuffle3(A, HTTPUACleaner.getRandomInt(c >> 3, c >> 1));

		return A.join("");
	},
	*/
	RandomStr: 			require('./getURL').RandomStr,
	getRandomIntShift: 	require('./getURL').getRandomIntShift,
	shuffle3: 			require('./getURL').shuffle3,

	hc1: [],
	toClean: 0,

	getHostNameClean: function()
	{
		var time = Date.now();

		// console.error("length s: " + HTTPUACleaner.hc1.length);
		for (var i = 0; i < HTTPUACleaner.hc1.length; i += 5)
		{
			try
			{
				if (	
						!HTTPUACleaner.hc1[i+4].isPending()
						&&
						(
								HTTPUACleaner.hc1[i+2] >= 1
							||  time - HTTPUACleaner.hc1[i+3] > 1 * 60 * 1000
						)
					)	// если запрос завершён
				{
					HTTPUACleaner.hc1.splice(i, 5);
					i -= 5;
				}
			}
			catch (ex)
			{
			}
		}
		// console.error("length e: " + HTTPUACleaner.hc1.length);
	},

	
	getHttpContext: function(httpRequest, httpRequestInterface, cached)
	{
		var lc = null;
		var num = HTTPUACleaner.hc1.indexOf(httpRequest);

		var time = Date.now();
		if (num >= 0)
		{
			if (!cached)
				HTTPUACleaner.hc1[num + 2]++;

			HTTPUACleaner.hc1[num + 3] = time;
		}

		var pushInterface = function(lc)
		{
			HTTPUACleaner.hc1.push(httpRequest);
			HTTPUACleaner.hc1.push(lc);
			HTTPUACleaner.hc1.push(0);
			HTTPUACleaner.hc1.push(time);
			HTTPUACleaner.hc1.push(httpRequestInterface);
		};

		getLC:
		{

			try
			{
				lc = httpRequest.notificationCallbacks.getInterface(Ci.nsILoadContext);
				if (lc)
				{
					if (num >= 0)
					{
						HTTPUACleaner.hc1[num + 1] = lc;
					}
					else
					{
						pushInterface(lc);
					}
					//console.error("0");
					break getLC;
				}
			}
			catch (ex)
			{
			}

			try
			{
				lc = httpRequest.loadGroup.notificationCallbacks.getInterface(Ci.nsILoadContext);
				if (lc)
				{
					if (num >= 0)
					{
						HTTPUACleaner.hc1[num + 1] = lc;
					}
					else
					{
						pushInterface(lc);
					}
					//console.error("1");
					break getLC;
				}
			}
			catch (ex)
			{
			}

			try
			{
				lc = httpRequestInterface.notificationCallbacks.getInterface(Ci.nsILoadContext);
				if (lc)
				{
					if (num >= 0)
					{
						HTTPUACleaner.hc1[num + 1] = lc;
					}
					else
					{
						pushInterface(lc);
					}
					//console.error("2");
					break getLC;
				}
			}
			catch (ex)
			{
			}

			try
			{
				lc = httpRequestInterface.loadGroup.notificationCallbacks.getInterface(Ci.nsILoadContext);
				if (lc)
				{
					if (num >= 0)
					{
						HTTPUACleaner.hc1[num + 1] = lc;
					}
					else
					{
						pushInterface(lc);
					}
					//console.error("3");
					break getLC;
				}
			}
			catch (ex)
			{
			}

		}

		try
		{
			if (!lc && num >= 0)
			{
				lc = HTTPUACleaner.hc1[num + 1];
				// console.error("OLD LOAD CONTEXT for " + httpRequestInterface.URI.spec);
			}
		}
		catch (ex)
		{
			console.error(ex);
		}

		
		//if (time - HTTPUACleaner.toClean > 15000)
		if (!httpRequestInterface.isPending())
		{
			HTTPUACleaner.toClean = time;
			HTTPUACleaner.getHostNameClean();
		}

		try
		{
		if (HTTPUACleaner.debug)
		{
			if (httpRequest.notificationCallbacks.owner)
			{
				console.error('httpRequest.notificationCallbacks.owner');
				console.error(httpRequest.notificationCallbacks);
				console.error(httpRequest.URI.spec);
			}
			
			if (httpRequest.loadGroup && httpRequest.loadGroup.notificationCallbacks.owner)
			{
				console.error('httpRequest.loadGroup.notificationCallbacks.owner');
				console.error(httpRequest.loadGroup.notificationCallbacks);
				console.error(httpRequest.URI.spec);
			}
		}
		}
		catch (e)
		{}

		return lc;	// loadContext.associatedWindow or topWindow to get Window object
	},

	
	toTwoLengthString: function(string)
	{
		if (string.length < 2)
			return "0" + string;
		else
			return string;
	},
	
	getRandomInt: require('./getURL').getRandomInt,
	getRandomValueByArray: require('./getURL').getRandomValueByArray,
	getRandomValueByArrayFreq: require('./getURL').getRandomValueByArrayFreq,


	getPriorityState: function(currentState, domainState, hostState, optionName, domain, host, nonGlobal)
	{
		if (!currentState)
			currentState = "none";
		if (!domainState)
			domainState = "none";
		if (!hostState)
			hostState = "none";
	
		var c  = currentState;

		var wc = 	HTTPUACleaner.enabledOptions[optionName				+ "Priority"] 	|| 5;
        var wd = (	HTTPUACleaner.enabledOptions[optionName + "Domain"  + "Priority"] 	|| {}	)[domain] 	|| 5;
		var wh = (	HTTPUACleaner.enabledOptions[optionName + "Domain"  + "Priority"] 	|| {}	)[host] 	|| 5;

		if (nonGlobal)
		{
			c  = nonGlobal;
			wc = 5;
		}

		if (wd <= wc && domainState != "none")
		{
			c = domainState;
			wc = wd;
		}

		if (wh <= wc && hostState != "none")
		{
			c = hostState;
		}

		return c;
	},

	initStorageOptions: function(httpOptions)
	{
		if (!httpOptions)
		{
			var prefs = HTTPUACleaner['sdk/preferences/service'];
			if (prefs.get(HTTPUACleaner_Prefix + 'simplestorage', true))
			{
				HTTPUACleaner.simplestorage = require("sdk/simple-storage"),
				HTTPUACleaner.storage = HTTPUACleaner.simplestorage.storage;

				if (!HTTPUACleaner.storage.enabledOptions && !HTTPUACleaner.storage.ciphersOptions)
				{
					HTTPUACleaner.storage = {};
					prefs.set(HTTPUACleaner_Prefix + 'simplestorage', false);
					console.error('HUAC information: the http options will loaded in future from the HUAC main settings file');
				}
				else
				{
					console.error('HUAC information: the http options loaded from sdk/simple-storage');
					console.error(HTTPUACleaner.simplestorage.storage);
				}
			}
			else
			{
				HTTPUACleaner.simplestorage = {};
				HTTPUACleaner.storage = {};
			}
		}
		else
		{
			HTTPUACleaner.storage = httpOptions;
		}

		if (!HTTPUACleaner.storage.enabledOptions)
		{
			HTTPUACleaner.storage.enabledOptions = {FontsHold: "enabled", PluginsHold: "enabled", CookiesHold: "disabled", UAHold: "low random", FontsDomain: {}, PluginsDomain: {}, CookiesDomain: {}, UADomain: {}};
		}

		for (var i = 3; i < HTTPUACleaner.mainOptionsNames.length; i++)
		{
			if (  !HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Hold"  ]  )
			{
				if (HTTPUACleaner.mainOptionsNames[i] == "UATI")
				{
					HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Hold"    ] = "disabled";
				}
				else
				if (HTTPUACleaner.mainOptionsNames[i] == "DNT")
				{
					HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Hold"    ] = "no track";
				}
				else
					HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Hold"    ] = "disabled";

				HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Domain"  ] = {};
			}
		}

		for (var i = 0; i < HTTPUACleaner.mainOptionsNames.length; i++)
		{
			if (  !HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "DomainPriority"  ]  )
			{
				HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "HoldPriority"    ] = "5";
				HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "DomainPriority"  ] = {};
			}
		}
		
		// Удаление неработающей функционально��ти plugins clean
		if (HTTPUACleaner.storage.enabledOptions[  "PluginsHold"  ] == 'random')
			HTTPUACleaner.storage.enabledOptions[  "PluginsHold"  ] = 'enabled';

		var domainsOptinos = HTTPUACleaner.storage.enabledOptions[  "PluginsDomain"  ];
		for (var domain in domainsOptinos)
		{
			if (HTTPUACleaner.storage.enabledOptions[  "PluginsDomain"  ][domain] == 'random')
				HTTPUACleaner.storage.enabledOptions[  "PluginsDomain"  ][domain] = 'enabled';
		}

		// обновление опций от старых версий
		for (var i = 0; i < HTTPUACleaner.mainOptionsNames.length; i++)
		{
			var status = HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Hold"  ];

			if (HTTPUACleaner.mainOptionsNames[i] == "hCookies" && !HTTPUACleaner.storage.enabledOptions["6.0.2"])
			{
				HTTPUACleaner.storage.enabledOptions[  "dCookiesHold"  ] 	= HTTPUACleaner.storage.enabledOptions[  "hCookiesHold"  ];

				if (HTTPUACleaner.storage.enabledOptions[  "hCookiesHold"  ] == "raise error")
				{
					HTTPUACleaner.storage.enabledOptions[  "hCookiesHold"  ] = "enabled";
				}
				
				if (HTTPUACleaner.storage.enabledOptions[  "dCookiesHold"  ] == 'host')
					HTTPUACleaner.storage.enabledOptions[  "dCookiesHold"  ] = "enabled";
			}

			if (status == "high enabled")
			{
				HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Hold"  ] = "enabled";
			}
			else
			if (status == "high random")
			{
				HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Hold"  ] = "random";
			}
			else
			if (status == "high disabled")
			{
				HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Hold"  ] = "disabled";
			}

			var domainsOptinos = HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Domain"  ];
			for (var domain in domainsOptinos)
			{
				status = HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Domain"  ][domain];
				if (status == "high enabled")
				{
					HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Domain"  ][domain] = "enabled";
				}
				else
				if (status == "high random")
				{
					HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Domain"  ][domain] = "random";
				}
				else
				if (status == "high disabled")
				{
					HTTPUACleaner.storage.enabledOptions[  HTTPUACleaner.mainOptionsNames[i] + "Domain"  ][domain] = "disabled";
				}
			}

			if (HTTPUACleaner.mainOptionsNames[i] == "hCookies" && !HTTPUACleaner.storage.enabledOptions["6.0.2"])
			{
				var domainsOptinos = HTTPUACleaner.storage.enabledOptions[  "hCookiesDomain"  ];
				for (var domain in domainsOptinos)
				{
					HTTPUACleaner.storage.enabledOptions[  "dCookiesDomain"  ][domain] = HTTPUACleaner.storage.enabledOptions[  "hCookiesDomain"  ][domain];
					if (HTTPUACleaner.storage.enabledOptions[  "hCookiesDomain"  ][domain] == "raise error")
					{
						HTTPUACleaner.storage.enabledOptions[  "hCookiesDomain"  ][domain] = "enabled";
					}

					if (HTTPUACleaner.storage.enabledOptions[  "dCookiesDomain"  ][domain] == "host")
						HTTPUACleaner.storage.enabledOptions[  "dCookiesDomain"  ][domain] = "enabled";
				}
				HTTPUACleaner.storage.enabledOptions["6.0.2"] = true;
			}
		}

		HTTPUACleaner.initEnabledOptions();
	},


	mainOptionsNames: ["Fonts", "Plugins"/*, "Cookies"*/, "UA", "Referer", "XForwardedFor", "Storage", "AcceptHeader", "Caching", "Etag", "OnlyHtml", "hCookies", "dCookies", "Images", "Audio", "WebRTC", "WebSocket", 'Fetch', "AJAX", "PushAPI", "ServiceWorker", "wname", "MUA", "UATI", 'DNT', "NoFilters", "Locale", 'Screen', 'OnlyHttps', 'TimeZone', 'CORS', 'WebGL', 'AudioCtx', 'Canvas', 'Authorization', 'Password'],
	mainOptionsColors: {"enabled": "#FF0000", "disabled": "#00BB00", "none": "#CCCCCC", "random": "#FFFF55", "low random": "#888855", "raise error": "0000FF", "no cache": "#FF00FF", "no persistent": "#0000FF", 'cache': '#000000', 'validate': '#FFFFFF', "html": "#FF00FF", "css/xml": "#FFFF00", "js": "#55FF00", "host": "#888855", "clean": "#FFFF00", "Firefox 28": "#FFFF00", "Opera 12.14": "#FFFF00", "Chrome 33": "#FFFF00", "IE 10.0": "#FFFF00", "Googlebot": "#FFFF00", "track": "#00AA00", "no track": "#FF0000", "1": "#FFFFFF", "2": "#DDFFFF", "3": "#BBFFFF", "4": "#99FFFF", "5": "#88FFFF", "6": "#77FFFF", "7": "#66FFFF", "8": "#55FFFF", "11": "#33FFFF", "no filters": "#FFFFFF", 'click': '#FFFF00', 'domain': '#AAAA77', 'isolated': '#C0C0C0', 'font': '#FFFFFF', 'en-us': '#FFFFFF'},

	// Дублируется в data/popupmenu.js за исключением строки "none", которая не должна встречаться в этом модуле
	mainOptionsValuesEnum: ["enabled", "disabled", "random", "low random", "raise error", "no cache", "no persistent", 'validate', 'cache', "html", "css/xml", "js", "host", "clean", "Firefox 28", "Opera 12.14", "Chrome 33", "IE 10.0", "Googlebot", "track", "no track", "1", "2", "3", "4", "5", "6", "7", "8", "11", "no filters", 'click', 'domain', 'isolated', 'font', 'en-us'],

	mainOptionsCaptions: {},
	mainOptionsCaptionsLocale: "none locale",
	mainOptionsCaptionsRefresh: function()
	{
		var getLocaleString = HTTPUACleaner['sdk/l10n'].get;

		for (var index in HTTPUACleaner.mainOptionsValuesEnum)
		{
			try
			{
				var name = HTTPUACleaner.mainOptionsValuesEnum[index];
				HTTPUACleaner.mainOptionsCaptions[name] = getLocaleString(name);
			}
			catch (e)
			{
				HTTPUACleaner.mainOptionsCaptions[name] = getLocaleString("LocaleError");
			};
		}

		HTTPUACleaner.mainOptionsCaptions['Reset FireFox tab settings to default']   = getLocaleString('Reset FireFox tab settings to default');
		HTTPUACleaner.mainOptionsCaptions['Reset HTTP tab settings to default']      = getLocaleString('Reset HTTP tab settings to default');
		HTTPUACleaner.mainOptionsCaptions['Reset Side tab']      					 = getLocaleString('Reset Side tab');
		HTTPUACleaner.mainOptionsCaptions['Clear Side tab history']     			 = getLocaleString('Clear Side tab history');
		HTTPUACleaner.mainOptionsCaptions['Reset data']     			 			 = getLocaleString('Reset data');
		HTTPUACleaner.mainOptionsCaptions['Certificates']     			 			 = getLocaleString('Certificates');

		var st = ['none', "Do to disable all", "Do to enable all"];
		for (var index in st)
		{
			try
			{
				var name = st[index];
				HTTPUACleaner.mainOptionsCaptions[name] = getLocaleString(name);
			}
			catch (e)
			{
				HTTPUACleaner.mainOptionsCaptions[name] = getLocaleString("LocaleError");
			};
		}

		HTTPUACleaner.mainOptionsCaptions['Firefox 28'] 	= 'Firefox 50';
		HTTPUACleaner.mainOptionsCaptions['Opera 12.14'] 	= 'Opera 40';
		HTTPUACleaner.mainOptionsCaptions['Chrome 33'] 		= 'Chrome 52';
		HTTPUACleaner.mainOptionsCaptions['IE 10.0'] 		= 'IE 11.0';
		HTTPUACleaner.mainOptionsCaptions['Googlebot'] 		= 'Googlebot/2.1';
	},

	initEnabledOptions: function()
	{
		var names = HTTPUACleaner.mainOptionsNames;
		for (var index in names)
		{
			var name = names[index];

			HTTPUACleaner.enabledOptions[name] 				= HTTPUACleaner.storage.enabledOptions[name + "Hold"];
			HTTPUACleaner.enabledOptions[name + "Hold"] 	= HTTPUACleaner.storage.enabledOptions[name + "Hold"];
			HTTPUACleaner.enabledOptions[name + "Domain"] 	= HTTPUACleaner.storage.enabledOptions[name + "Domain"];

			HTTPUACleaner.enabledOptions[name 			 + "Priority"] 	= HTTPUACleaner.storage.enabledOptions[name + "Hold"	+ "Priority"];
			HTTPUACleaner.enabledOptions[name + "Hold"	 + "Priority"] 	= HTTPUACleaner.storage.enabledOptions[name + "Hold"	+ "Priority"];
			HTTPUACleaner.enabledOptions[name + "Domain" + "Priority"] 	= HTTPUACleaner.storage.enabledOptions[name + "Domain"	+ "Priority"];
		}

	},

	httpRequestObserved: new Array(),
	
	getTabUri: function(ch)
	{
		var obj         = {deleteRequestFromArray: true};
		var httpChannel = ch.QueryInterface(Ci.nsIHttpChannel);
		var context     = HTTPUACleaner.getHttpContext(ch, httpChannel);
		var host        = HTTPUACleaner.getHostName(httpChannel, ch, true, context, obj);

		var urls = HTTPUACleaner.urls; // require('./getURL');
		var tab = context ? urls.getBrowserForContext(context) : '';
		var uri = context ? urls.fromBrowser(tab) : httpChannel.URI.spec;
		if (obj.taburl)
		{
			// console.error('to taburl ' + uri + ' from ' + uri);
			uri = obj.taburl; //obj.HTTPUACleaner_URI;
		}

		return uri;
	},

	deleteRequestFromArray_SetInterval: function(isSetInterval)
	{
		if (isSetInterval)
		{
			HTTPUACleaner.deleteRequestFromArray_SetInterval_intervalId = HTTPUACleaner.timers.setInterval
			(
				function()
				{
					try
					{
						if (HTTPUACleaner.terminated !== false)
							return;

						HTTPUACleaner.deleteRequestFromArray();
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}

					try
					{
						HTTPUACleaner.certsObject.saveSettingDecision(Date.now(), false, true);
					}
					catch (e)
					{
						HTTPUACleaner.logObject(e, true);;
					}
				},
				1021
			);
		}
		else
			HTTPUACleaner.timers.clearInterval(HTTPUACleaner.deleteRequestFromArray_SetInterval_intervalId);
	},

	deleteRequestFromArray: function()
	{
		for (var i = 0; i < HTTPUACleaner.httpRequestObserved.length; i++)
		{
			var ch     = HTTPUACleaner.httpRequestObserved[i];
			var status = ch.status;

			if (HTTPUACleaner.debugOptions && HTTPUACleaner.debugOptions.httpRequestObserved)
			{
				console.error('httpRequestObserved ' + i);

				try
				{
					console.error(ch.URI.spec);
					console.error('is pending ' + ch.isPending());
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
				}

				console.error(ch.status.toString(16).toUpperCase() + 'h');

				for (var crName in Cr)
				{
					if (Cr[crName] == status)
					{
						console.error(crName);
						break;
					}
				}
			}

			if (status == 0x805A1FF3 || status == 0x805A4000)
			{
				console.error('HUAC information: In the request was found the error of loading, this may be relevance to HPKP');
				console.error(ch.URI.spec);
				console.error('status code: ' + ch.status.toString(16).toUpperCase() + 'h');

				if (!HTTPUACleaner.HPKP_ResetArray)
					HTTPUACleaner.HPKP_ResetArray = {};

				// Устанавливаем флаг для того, чтобы мож��о было сбросить информацию для HPKP, если фильтр установлен в сброс
				HTTPUACleaner.HPKP_ResetArray[ch.URI.host] = true;

				var taburi = '';
				try
				{
					taburi = HTTPUACleaner.getTabUri(ch);
				}
				catch (e)
				{}


				var obj = {type: 'certs autodisable', level: 3, msg: {description: 'In the request was found the error of loading, this may be relevance to HPKP or certs autodisable'}};

				HTTPUACleaner.loggerB.addToLog(taburi, false, ch.URI ? ch.URI.spec : 'ch.URI is undefined', obj);
			}


			// если запрос завершён или было перенаправление раньше, чем запрос начался
			if (!ch.isPending() || status == Cr.NS_BINDING_REDIRECTED || status == 0x805A1FF3 || status == 0x805A4000)
			{
				if (HTTPUACleaner.httplog.enabled)
				try
				{
					var httpChannel = ch.QueryInterface(Ci.nsIHttpChannel);
					var context  = HTTPUACleaner.getHttpContext(ch, httpChannel);
					HTTPUACleaner.LogResponseHeaders(ch, httpChannel, 0, 0, 0, context, 0, 0);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
				}
				
				HTTPUACleaner.httpRequestObserved.splice(i, 1);
				i--;

				if (HTTPUACleaner.debugOptions && HTTPUACleaner.debugOptions.httpRequestObserved)
				{
					console.error('deleted1 ' + (i+1));
				}

				continue;
			}


			if (ch.isPending() && status != 0
				/*&& status != Cr.NS_BINDING_REDIRECTED && status != Cr.NS_ERROR_LOSS_OF_SIGNIFICANT_DATA
				&& status != Cr.NS_ERROR_NOT_INITIALIZED && status != Cr.NS_ERROR_NOT_IMPLEMENTED
				&& status != Cr.NS_ERROR_CACHE_READ_ACCESS_DENIED
				&& status != Cr.NS_BINDING_ABORTED*/
				)
			{
				if (HTTPUACleaner.httplog.enabled)
				try
				{
					var httpChannel = ch.QueryInterface(Ci.nsIHttpChannel);
					var context  = HTTPUACleaner.getHttpContext(ch, httpChannel);
					HTTPUACleaner.LogResponseHeaders(ch, httpChannel, 0, 0, 0, context, 0, 0);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
				}
				
				HTTPUACleaner.httpRequestObserved.splice(i, 1);
				i--;

				if (HTTPUACleaner.debugOptions && HTTPUACleaner.debugOptions.httpRequestObserved)
				{
					console.error('deleted2 ' + (i+1));
				}

				// Пропускаем редиректы и то, что может дать при блокировках само дополнение
				/*if (status == Cr.NS_BINDING_REDIRECTED || status == Cr.NS_ERROR_LOSS_OF_SIGNIFICANT_DATA
				|| status == Cr.NS_ERROR_NOT_INITIALIZED || status == Cr.NS_ERROR_NOT_IMPLEMENTED
				|| status == Cr.NS_ERROR_CACHE_READ_ACCESS_DENIED)
					continue;*/

				var flag = status;
				for (var crName in Cr)
				{
					if (Cr[crName] == status)
					{
						flag = crName;
						break;
					}
				}

				if (status != Cr.NS_BINDING_REDIRECTED && status != Cr.NS_ERROR_LOSS_OF_SIGNIFICANT_DATA
				&& status != Cr.NS_ERROR_NOT_INITIALIZED && status != Cr.NS_ERROR_NOT_IMPLEMENTED
				&& status != Cr.NS_ERROR_CACHE_READ_ACCESS_DENIED
				&& status != Cr.NS_BINDING_ABORTED)
				{
					console.error('HUAC information: status of http request');
					console.error(ch.URI.spec);
					console.error(flag);
				}

				var taburi = '';
				try
				{
					taburi = HTTPUACleaner.getTabUri(ch);
				}
				catch (e)
				{}

				var obj = {type: 'network status', level: 3, msg: {'Fail status': flag}};

				HTTPUACleaner.loggerB.addToLog(taburi, false, ch.URI ? ch.URI.spec : 'ch.URI is undefined', obj);

				continue;
			}
		}

		// console.error("length: " + HTTPUACleaner.httpRequestObserved.length);
	},

	isNoCPProtocol: function(protocol, noHighLevel)
	{
		if (!noHighLevel)
		{
			if (
				   protocol == "resource"
				|| protocol == "data"
				|| protocol == "about"
				|| protocol == "moz-safe-about"
				|| protocol == "moz-filedata"
				|| protocol == "moz-icon"
				|| protocol == "chrome"
				|| protocol == "blob"
				|| protocol == "view-source"
				|| protocol == "javascript"
			)
				return true;
		}
		else
		if (
						   protocol == "data"
						|| protocol == "blob"
						|| protocol == "javascript"
					)
			return true;

		return false;
	},

	setPreferences: function()
	{
		var prefs = HTTPUACleaner['sdk/preferences/service'];
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'EstimateTLS'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'EstimateTLS', true)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'logb.enabled'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'logb.enabled', true)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'minTLSStrong'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'minTLSStrong', 1)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'certs.hosts'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'certs.hosts', false)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'certs.hosts.private'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'certs.hosts.private', false)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'certs.hosts.TimeOnly'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'certs.hosts.TimeOnly', false)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'certs.hosts.TimeOnly.private'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'certs.hosts.TimeOnly.private', false)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'certs.hostsopt.autodisable'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'certs.hostsopt.autodisable', false)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'resourceDisallow'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'resourceDisallow', true)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'resourceDisallowStrong'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'resourceDisallowStrong', false)
		}
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'ocspAllowedOnStart'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'ocspAllowedOnStart', false)
		}
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'hostsAllowedOnStart'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'hostsAllowedOnStart', '')
		}

		HTTPUACleaner.ocspAllowedOnStart  = prefs.get(HTTPUACleaner_Prefix + 'ocspAllowedOnStart',  false);

		HTTPUACleaner.hostsAllowedOnStart = {};
		// shavar.services.mozilla.com addons.cdn.mozilla.net self-repair.mozilla.org
		var ah = prefs.get(HTTPUACleaner_Prefix + 'hostsAllowedOnStart', '');
		ah = ah.split(' ');
		for (var h of ah)
			if (h)
			{
				HTTPUACleaner.hostsAllowedOnStart[h] = true;
				if (h.startsWith('.'))
					HTTPUACleaner.hostsAllowedOnStart[h.substring(1)] = true;
			}


		HTTPUACleaner.EstimateTLS        = prefs.get(HTTPUACleaner_Prefix + 'EstimateTLS',  true);
		HTTPUACleaner.resourceDisallow   = prefs.get(HTTPUACleaner_Prefix + 'resourceDisallow', true);
		HTTPUACleaner.resourceDisallowStrong
										 = prefs.get(HTTPUACleaner_Prefix + 'resourceDisallowStrong', false);
		HTTPUACleaner.loggerB.enabled    = prefs.get(HTTPUACleaner_Prefix + 'logb.enabled', true);
		HTTPUACleaner.minTLSStrong       = prefs.get(HTTPUACleaner_Prefix + 'minTLSStrong', true);
		HTTPUACleaner.loggerSide.enabled = prefs.get(HTTPUACleaner_Prefix + 'SideTable.enabled', true);
		HTTPUACleaner.html.font          = prefs.get(HTTPUACleaner_Prefix + 'mainpanel.font', '16px "Times New Roman"');
		HTTPUACleaner.certsObject.hosts	 = prefs.get(HTTPUACleaner_Prefix + 'certs.hosts', false);
		HTTPUACleaner.certsObject.hostsPrivate = prefs.get(HTTPUACleaner_Prefix + 'certs.hosts.private', false);
		HTTPUACleaner.certsObject.hostsTO = prefs.get(HTTPUACleaner_Prefix + 'certs.hosts.TimeOnly', false);
		HTTPUACleaner.certsObject.hostsTOPrivate = prefs.get(HTTPUACleaner_Prefix + 'certs.hosts.TimeOnly.private', false);
		HTTPUACleaner.certsObject.hostsOpt = {};
		HTTPUACleaner.certsObject.hostsOpt.autodisable = prefs.get(HTTPUACleaner_Prefix + 'certs.hostsopt.autodisable', false);
		
		
		HTTPUACleaner.truncLenght = {};

		if (!prefs.isSet(HTTPUACleaner_Prefix + 'truncLenght.url'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'truncLenght.url', 100)
		}
		HTTPUACleaner.truncLenght.url = Number(prefs.get(HTTPUACleaner_Prefix + 'truncLenght.url', 100));
		
		HTTPUACleaner.truncLenght.httplog = {};
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'truncLenght.httplog.data'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'truncLenght.httplog.data', 1024)
		}
		HTTPUACleaner.truncLenght.httplog.data = Number(prefs.get(HTTPUACleaner_Prefix + 'truncLenght.httplog.data', 1024));
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'truncLenght.httplog.info'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'truncLenght.httplog.info', 0)
		}
		HTTPUACleaner.truncLenght.httplog.info = Number(prefs.get(HTTPUACleaner_Prefix + 'truncLenght.httplog.info', 0));
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'truncLenght.httplog.infoI'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'truncLenght.httplog.infoI', 0)
		}
		HTTPUACleaner.truncLenght.httplog.infoI = Number(prefs.get(HTTPUACleaner_Prefix + 'truncLenght.httplog.infoI', 0));

		if (!prefs.isSet(HTTPUACleaner_Prefix + 'truncLenght.side'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'truncLenght.side', false)
		}
		HTTPUACleaner.truncLenght.side = prefs.get(HTTPUACleaner_Prefix + 'truncLenght.side', false);


		if (!prefs.isSet(HTTPUACleaner_Prefix + 'certs.HPKP.minMaxAge'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'certs.HPKP.minMaxAge', 45)
		}
		HTTPUACleaner.certsObject.HPKP_minMaxAge = Number(prefs.get(HTTPUACleaner_Prefix + 'certs.HPKP.minMaxAge', 45));
		
		if (!prefs.isSet(HTTPUACleaner_Prefix + 'certs.HPKP.minMaxAgeReplace'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'certs.HPKP.minMaxAgeReplace', 45)
		}
		HTTPUACleaner.certsObject.HPKP_minMaxAgeReplace = Number(prefs.get(HTTPUACleaner_Prefix + 'certs.HPKP.minMaxAgeReplace', 45));

		HTTPUACleaner.debugOptions = {};
		HTTPUACleaner.debugOptions.httpRequestObserved = prefs.get(HTTPUACleaner_Prefix + 'debug.console.httpRequestObserved', false);

		HTTPUACleaner.debugOptions.canvasToSee = prefs.get(HTTPUACleaner_Prefix + 'debug.canvasToSee', false);
		HTTPUACleaner.debugOptions.hpkpdigest  = prefs.get(HTTPUACleaner_Prefix + 'debug.hpkpdigest',  false);

		HTTPUACleaner.debugOptions.IncorrectCertificateLevel = prefs.get(HTTPUACleaner_Prefix + 'debug.IncorrectCertificateLevel', false);


		if (!prefs.isSet(HTTPUACleaner_Prefix + 'httplog.enabled'))
		{
			prefs.set(HTTPUACleaner_Prefix + 'httplog.enabled', false)
		}
		
		HTTPUACleaner.httplog = {};
		HTTPUACleaner.httplog.enabled = prefs.get(HTTPUACleaner_Prefix + 'httplog.enabled', false);

		
		HTTPUACleaner.certsObject.setInterval();

		HTTPUACleaner.setE10S();
	},

	disableE10S: function()
	{
		var prefs = HTTPUACleaner['sdk/preferences/service'];

		if (!prefs.get(HTTPUACleaner_Prefix + 'e10sfalse',  false))
			return false;
		
		HTTPUACleaner.setE10S_informed = false;

		if (!HTTPUACleaner.isE10SA)
			return 0;

		prefs.set('browser.tabs.remote.autostart',  false);
		prefs.set('browser.tabs.remote.autostart.1', false);
		prefs.set('browser.tabs.remote.autostart.2', false);
		
		return true;
	},
	
	startupRegisterHttpEvents: function()
	{
		var version = HTTPUACleaner.version;
		if (version.indexOf('a') > 0 || version.indexOf('b') > 0 || version.indexOf('rc') > 0)
		{
			HTTPUACleaner.debug = true;
		}

		HTTPUACleaner.setObserver();

		var events = require("sdk/system/events");
		events.on("http-on-opening-request", 		 HTTPUACleaner.observer.observe);
		events.on("http-on-modify-request", 		 HTTPUACleaner.observer.observe);
		events.on("http-on-examine-response", 		 HTTPUACleaner.observer.observe);
		events.on("http-on-examine-cached-response", HTTPUACleaner.observer.observe);
		events.on("http-on-examine-merged-response", HTTPUACleaner.observer.observe);
	},
	
	allBlock: -1,
	
	// НЕ РАБОТАЕТ
	onCookiesChanged: function(e)
	{/*
		// Нам не нужна эта функция
		return;

		if (e.data != 'added')
			return;

		var prv = true;
		if (e.type == 'cookie-changed')
		{
			prv = false;
		}
		else
		if (e.type == 'private-cookie-changed')
		{
			prv = true;
		}
		else
		{
			console.error('HUAC FATAL ERROR: onCookiesChanged type not undefinite');
			console.error(arguments);

			return;
		}
		
		var cookieFunc = function(ck)
		{
			try
			{
				var ck2 = ck.QueryInterface(Ci.nsICookie2);
				// console.error(ck2);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}
		};
		
		if (e.subject instanceof Array)
		{
			for (var a of e.subject)
				cookieFunc(a);
		}
		else
			cookieFunc(e.subject);*/
	},

	httpOptionsInitialized: function()
	{
		if (HTTPUACleaner.allBlock === false || HTTPUACleaner.allBlock >= 3)
			return true;

		return false;
	},
	
    startup: function ()
    {
		Cu.import("resource://gre/modules/FileUtils.jsm", HTTPUACleaner);
		Cu.import("resource://gre/modules/NetUtil.jsm",   HTTPUACleaner);

		HTTPUACleaner.startupRegisterHttpEvents();
		
		for (var wIndex in HTTPUACleaner.words)
		{
			HTTPUACleaner.words[wIndex] = HTTPUACleaner['sdk/l10n'].get(HTTPUACleaner.words[wIndex]);
		}

		HTTPUACleaner.setPreferences();

		HTTPUACleaner.executedTime = Date.now();

		//HTTPUACleaner.openIndexedDBLog();
		HTTPUACleaner.MainMenu.create();

		try
		{
			var registrar = Cm.QueryInterface(Ci.nsIComponentRegistrar);
			registrar.registerFactory(HTTPUACleaner.observer._classID, HTTPUACleaner.observer._classDescription, HTTPUACleaner.observer._contractID, HTTPUACleaner.observer);
		}
		catch (e)
		{
			var be = HTTPUACleaner['sdk/l10n'].get('botherror');
			if (be.length <= 0 || be == 'botherror')
				be = 'The HTTP UserAgent Cleaner extension has been executed with error. This error often occured if you executed two HUAC extensions at the same time (Gallery and Site versions). Delete one of extensions copies for a normal work.';

			console.error('HUAC FATAL ERROR: ' + be);
			HTTPUACleaner.logObject(e, true);;

			HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('botherror.html'), undefined, undefined, true);
			return;
		}

		try
		{
			var catMan = Cc["@mozilla.org/categorymanager;1"].getService(Ci.nsICategoryManager);
			catMan.addCategoryEntry("content-policy", HTTPUACleaner.observer._contractID, HTTPUACleaner.observer._contractID, false, true);
			
			/*
			else
			{
				consoleJSM.console.logp('simple-content-policy category in use');
				catMan.addCategoryEntry("simple-content-policy", HTTPUACleaner.observer._contractID, HTTPUACleaner.observer._contractID, false, true);
			}*/
		}
		catch (e)
		{
			HTTPUACleaner.logObject(e, true);;
		}
		
		
		var events = require("sdk/system/events");
		events.on("document-element-inserted", 		 HTTPUACleaner.onDocumentCreated);
		// events.on("cookie-changed", 		 		 HTTPUACleaner.onCookiesChanged);
		// events.on("private-cookie-changed", 		 HTTPUACleaner.onCookiesChanged);
		events.on("last-pb-context-exited", 		 HTTPUACleaner.endPrivateBrowsing);
		/*events.on("http-on-opening-request", 		 HTTPUACleaner.observer.observe);
		events.on("http-on-modify-request", 		 HTTPUACleaner.observer.observe);
		events.on("http-on-examine-response", 		 HTTPUACleaner.observer.observe);
		events.on("http-on-examine-cached-response", HTTPUACleaner.observer.observe);
		events.on("http-on-examine-merged-response", HTTPUACleaner.observer.observe);*/

		HTTPUACleaner.setOtherExtensionsForWell();

		/*
		for (var tab of require("sdk/tabs"))
			HTTPUACleaner.registerBrowserBySdkTab(tab);
*/
		if (HTTPUACleaner.isE10S)
		{

		}
		
		var pageMod = require("sdk/page-mod");
		pageMod.PageMod
		({
			include: HTTPUACleaner['sdk/self'].data.url('certs/main.html'),
			contentScriptFile: 
								[
								HTTPUACleaner['sdk/self'].data.url('certs/scripts.js'),
								HTTPUACleaner['sdk/self'].data.url('certs/view.js')
								],
			onAttach: function(worker)
			{
				worker.port.on
				(
					"cert",
					function(args)
					{
						HTTPUACleaner.certsObject.event(worker, args);
					}
				);

				HTTPUACleaner.certsObject.showAll(worker);
			}
		});

		pageMod.PageMod
		({
			include: HTTPUACleaner['sdk/self'].data.url('certs/hosts.html'),
			contentScriptFile: 
								[
								HTTPUACleaner['sdk/self'].data.url('certs/scripts.js'),
								HTTPUACleaner['sdk/self'].data.url('certs/view.js')
								],
			onAttach: function(worker)
			{
				worker.port.on
				(
					"certHosts",
					function(args)
					{
						HTTPUACleaner.certsObject.eventHosts(worker, args);
					}
				);

				HTTPUACleaner.certsObject.showAllHosts(worker);
			}
		});
		
		pageMod.PageMod
		({
			include: HTTPUACleaner['sdk/self'].data.url('certs/failure.html'),
			contentScriptFile:
								[
								HTTPUACleaner['sdk/self'].data.url('certs/scripts.js'),
								HTTPUACleaner['sdk/self'].data.url('certs/view.js')
								],
			onAttach: function(worker)
			{
				worker.port.on
				(
					"certFailure",
					function(args)
					{
						HTTPUACleaner.certsObject.eventFailure(worker, args);
					}
				);

				HTTPUACleaner.certsObject.showAllFailures(worker);
			}
		});

		pageMod.PageMod
		({
			include: HTTPUACleaner['sdk/self'].data.url('blocked.html') + '?*',
			contentScriptFile:
								[
								HTTPUACleaner['sdk/self'].data.url('blocked.js')
								],
			onAttach: function(worker)
			{
				worker.port.on
				(
					"blockedUrl",
					function(args)
					{
						worker.port.emit
						(
							"blockedUrl",
							{
								url: HTTPUACleaner.querystring.unescape(args.url)
							}
						);
					}
				);

				worker.port.emit
				(
					"blockedUrl",
					{}
				);
			}
		});

		pageMod.PageMod
		({
			include: HTTPUACleaner['sdk/self'].data.url('httplog.html'),
			contentScriptFile:
								[
								HTTPUACleaner['sdk/self'].data.url("mainjs/logBView.js"),
								HTTPUACleaner['sdk/self'].data.url('httplog.js')
								],
			onAttach: function(worker)
			{
				worker.port.on
				(
					"showHLog", 
					function(opt)
					{
						HTTPUACleaner.getHLog(opt, worker);
					}
				);

				HTTPUACleaner.getHLog(true, worker);
			}
		});

		try
		{
			HTTPUACleaner.deleteRequestFromArray_SetInterval(true);
		}
		catch (e)
		{
			HTTPUACleaner.logObject(e, true);;
		}

		HTTPUACleaner.terminated = false;
		if (HTTPUACleaner.allBlock !== false)
		{
			HTTPUACleaner.allBlock++;
			HTTPUACleaner.setPluginButtonState();
		}

		HTTPUACleaner.initStorageOptions();
		HTTPUACleaner.startupTime = Date.now();
    },

	// В начале и в конце terminated обозначает не до конца инициализированное состояние
	terminated: true,
    shutdown: function (uninstall)
    {
		HTTPUACleaner.terminated = true;

		// Это и так сохраняется при изменениях и непригодно для асинхронного сохранения при окончании работы
		// HTTPUACleaner.sdb.setOptions(this.settings);

		HTTPUACleaner.certsObject.clearInterval(uninstall);
		try
		{
			HTTPUACleaner.deleteRequestFromArray_SetInterval(false);
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('shutdown exception in deleteRequestFromArray_SetInterval');
			HTTPUACleaner.logObject(e, true);
		}

		if (HTTPUACleaner.isE10S)
		{
		}

		try
		{
			HTTPUACleaner.addPrefsObserver(false);
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('shutdown exception in addPrefsObserver(false)');
			HTTPUACleaner.logObject(e, true);
		}

		try
		{
			var events = require("sdk/system/events");
			events.off("document-element-inserted", 		HTTPUACleaner.onDocumentCreated);
			events.off("http-on-opening-request", 		 	HTTPUACleaner.observer.observe);
			events.off("http-on-modify-request", 		 	HTTPUACleaner.observer.observe);
			events.off("http-on-examine-response", 		 	HTTPUACleaner.observer.observe);
			events.off("http-on-examine-cached-response", 	HTTPUACleaner.observer.observe);
			events.off("http-on-examine-merged-response", 	HTTPUACleaner.observer.observe);
			events.off("last-pb-context-exited", 			HTTPUACleaner.endPrivateBrowsing);
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('shutdown exception in events');
			HTTPUACleaner.logObject(e, true);
		}
		
		try
		{
			var catMan = Cc["@mozilla.org/categorymanager;1"].getService(Ci.nsICategoryManager);
			catMan.deleteCategoryEntry("content-policy", HTTPUACleaner.observer._contractID, false);
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('shutdown exception in deleteCategoryEntry');
			HTTPUACleaner.logObject(e, true);
		}

		try
		{
			var registrar = Cm.QueryInterface(Ci.nsIComponentRegistrar);
			registrar.unregisterFactory(HTTPUACleaner.observer._classID, HTTPUACleaner.observer);
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('shutdown exception in unregisterFactory');
			HTTPUACleaner.logObject(e, true);
		}
		
		try
		{
			var urls = HTTPUACleaner.urls;
			var tabs = urls.tabs;

			tabs.removeListener("open", 	 HTTPUACleaner.handleTabOpen);
			tabs.removeListener("ready", 	 HTTPUACleaner.handleTabReady);
			tabs.removeListener("activate",  HTTPUACleaner.handleTabActivate);
			tabs.removeListener("close",     HTTPUACleaner.handleTabClosing);
			
			for (var tabIndex in tabs)
			{
				try
				{
					var tab = tabs[tabIndex];
					var tb = urls.sdkTabToChromeTab(tab);
					if (tb && tb.linkedBrowser && tb.linkedBrowser.huac)
						delete tb.linkedBrowser.huac;
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);;
				}
			}
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('shutdown exception in tabs.removeListener');
			HTTPUACleaner.logObject(e, true);
		}

		try
		{
			HTTPUACleaner.endPrivateBrowsing();
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('shutdown exception in endPrivateBrowsing');
			HTTPUACleaner.logObject(e, true);
		}

		try
		{
			// Cu.unload(HTTPUACleaner['sdk/self'].data.url('mainjs/urlFunction.js'));
			HTTPUACleaner.urls.unload();
		}
		catch (e)
		{
			if (e.name != 'NS_ERROR_NOT_AVAILABLE')
			{
				HTTPUACleaner.logMessage('shutdown exception in Cu.unload mainjs/urlFunction.js');
				HTTPUACleaner.logObject(e, true);
			}
		}
    },

	enabled: true,
	enabledOptions: {},
	
	disablePlugin: function(enabled)
	{
		HTTPUACleaner.enabled = enabled === true;
		HTTPUACleaner.panel.hide();

		HTTPUACleaner.deleteRequestFromArray();
		HTTPUACleaner.endPrivateBrowsing();

		HTTPUACleaner.setPluginButtonState();
	},
	
	setPluginButtonState: function()
	{
		if (!HTTPUACleaner.ToggleButton)
			return;

		if (!HTTPUACleaner.enabled)
			HTTPUACleaner.ToggleButton.state(HTTPUACleaner.ToggleButton, {icon: {'32': "./HTTPUACleaner_disabled.png"}});
		else
		if (HTTPUACleaner.terminated || !HTTPUACleaner.httpOptionsInitialized())
		{
			HTTPUACleaner.ToggleButton.state(HTTPUACleaner.ToggleButton, {icon: {'32': "./HTTPUACleaner_notinitialized.png"}});
		}
		else
		if (HTTPUACleaner.getFunctionState(undefined, "NoFilters") == 'no filters')
			HTTPUACleaner.ToggleButton.state(HTTPUACleaner.ToggleButton, {icon: {'32': "./HTTPUACleaner_fdisabled.png"}});
		else
			HTTPUACleaner.ToggleButton.state(HTTPUACleaner.ToggleButton, {icon: {'32': "./HTTPUACleaner.png"}});
	},

	clickOnOptions: function(eventArgs)
	{
		if (!HTTPUACleaner.httpOptionsInitialized())
		{
			console.error("HUAC has been not initialized (clickOnOptions)");
			return false;
		}

		//HTTPUACleaner.logObject(eventArgs);

		var name = eventArgs.name;
		var newState = eventArgs.newState;
		var postfix  = "";
		var isDomainOptions = false;
		if (eventArgs.special == "main")
		{
		}
		else
		if (eventArgs.special == "Hold")
		{
			postfix = "Hold";
		}
		else
		{
			isDomainOptions = true;
			postfix = "Domain";
		}

		if (isDomainOptions)
		{
			if (newState == "none")
			{
				delete (HTTPUACleaner.enabledOptions		 [name + postfix])[eventArgs.currentDomain];
				delete (HTTPUACleaner.storage.enabledOptions [name + postfix])[eventArgs.currentDomain];

				// HTTPUACleaner.logMessage('deleted');

				HTTPUACleaner.sdb.setHTTP();
			}
			else
			{
				HTTPUACleaner.enabledOptions		[name + postfix][eventArgs.currentDomain] = newState;
				HTTPUACleaner.storage.enabledOptions[name + postfix][eventArgs.currentDomain] = newState;

				// HTTPUACleaner.logMessage('newState = ' + newState);

				HTTPUACleaner.sdb.setHTTP();
			}
		}
		else
		{
			HTTPUACleaner.enabledOptions[name + postfix] = newState;

			// HTTPUACleaner.logMessage('no domain ' + postfix);

			if (postfix && postfix.length > 0 && name != "MUA")	// MUA - р��жим, который не запоминается
			{
				HTTPUACleaner.storage.enabledOptions[name + postfix] = newState;

				// HTTPUACleaner.logMessage('save to storage ' + name + postfix);

				HTTPUACleaner.sdb.setHTTP();
			}
		}

		HTTPUACleaner.setPluginButtonState();
	},
	
	clickOnPriorityOptions: function(eventArgs)
	{
		if (!HTTPUACleaner.httpOptionsInitialized())
		{
			console.error("HUAC has been not initialized (clickOnPriorityOptions)");
			return false;
		}

		var name = eventArgs.name;
		var newState = eventArgs.newState;
		var postfix  = "Priority";
		var isDomainOptions = false;
		if (eventArgs.special == "")
		{
		}
		else
		if (eventArgs.special == "Hold")
		{
			postfix = "HoldPriority";
		}
		else
		{
			isDomainOptions = true;
			postfix = "DomainPriority";
		}

		if (isDomainOptions)
		{
			HTTPUACleaner.enabledOptions		[name + postfix][eventArgs.currentDomain] = newState;
			HTTPUACleaner.storage.enabledOptions[name + postfix][eventArgs.currentDomain] = newState;
			
			HTTPUACleaner.sdb.setHTTP();
		}
		else
		{
			HTTPUACleaner.enabledOptions[name + postfix] = newState;
			if (postfix != "Priority")
			{
				HTTPUACleaner.storage.enabledOptions[name + postfix] = newState;
				HTTPUACleaner.sdb.setHTTP();
			}
		}
	},

	ciphersCodes: 
	{
		"DES3": 			[{i: ["_des_"], 		e: []}],
		"RC4": 				[{i: ["_rc4_"], 		e: []}],
		"chacha20": 		[{i: ["_chacha20_"], 	e: []}],
		"AES128": 			[{i: ["_aes_128"], 		e: ["_aes_128_gcm", "_aes_256_gcm"]}],
		"AES128GCM": 		[{i: ["_aes_128_gcm"],	e: []}],
		"AES": 				[{i: ["_aes_256"], 		e: ["_aes_128_gcm", "_aes_256_gcm"]}],
		"AES256GCM": 		[{i: ["_aes_256_gcm"],	e: []}],
		"MD5":				[{i: ["_md5"], 			e: []}],
		"SHA1":				[{i: ["_sha"], 			e: ["_sha256", "_sha384", "_sha512"]}],
		"SHA256":			[{i: ["_sha256"],		e: []}],
		"SHA384":			[{i: ["_sha384"],		e: []}],
		//"DSS": 				[{i: ["dss_"], 			e: []}],
		"RSA": 				[{i: ["rsa_"], 			e: ["dhe_"]}],
		"RSA_PFS":			[{i: ["e_rsa_"], 		e: []}],
		"DHE": 				[{i: ["dhe_"], 			e: ["ecdhe"]}],
		"ECDHE": 			[{i: ["ecdhe_"], 		e: []}],
		"ECDSA": 			[{i: ["ecdsa_"], 		e: []}],
		"poly1305": 		[{i: ["poly1305"], 		e: []}]
	},
	
	RestoreAllCiphersState: function(doNotRestore)
	{
		if (!HTTPUACleaner.httpOptionsInitialized())
		{
			console.error("HUAC has been not initialized (RestoreAllCiphersState) " + doNotRestore);
			return false;
		}

		if (HTTPUACleaner.NoNeedRestoreAllCiphersState === true)
			return;

		if (HTTPUACleaner.doNotRestoreAllCiphersState_supress != 1)
		{
			if (doNotRestore)
			{
				HTTPUACleaner.doNotRestoreAllCiphersState_supress = 0;
			}
			else
				HTTPUACleaner.doNotRestoreAllCiphersState_supress = 2;
		}

		var needToSave = false;
		var result = false;
		for (var code in HTTPUACleaner.ciphersCodes)
		{
			var i = HTTPUACleaner.storage.ciphersOptions.enabled.indexOf(code);
			var j = HTTPUACleaner.storage.ciphersOptions.disabled.indexOf(code);

			if (i >= 0 && j >= 0)
			{
				var errMsg = 'HUAC ERROR: RestoreAllCiphersState. Bad ciphersOptions settings.';
				console.error(errMsg);

				HTTPUACleaner.logMessage(errMsg);
				HTTPUACleaner.logCallers();
				HTTPUACleaner.logObject(HTTPUACleaner.storage.ciphersOptions.enabled);
				HTTPUACleaner.logObject(HTTPUACleaner.storage.ciphersOptions.disabled);

				result = true;
			}

			if (i >= 0)
			{
				needToSave = true;
				if (HTTPUACleaner.clickOnCipherOptions({cName: code, newState: "+", doNotRestore: doNotRestore}, true))
					result = true;
			}

			if (j >= 0)
			{
				needToSave = true;
				if (HTTPUACleaner.clickOnCipherOptions({cName: code, newState: "-", doNotRestore: doNotRestore}, true))
					result = true;
			}

			if (i < 0 && j < 0)
			{
				// Вводим разрешение на новый шифр
				if (code == 'chacha20' || code == 'poly1305')
				{
					needToSave = true;
					HTTPUACleaner.storage.ciphersOptions.enabled.push(code);
				}
			}
		}

		if (needToSave)
		{
			HTTPUACleaner.sdb.setHTTP();

			HTTPUACleaner.panel.port.emit
			(
				"updateCiphers",
				HTTPUACleaner.getCiphers()
			);
		}

		if (result === true && HTTPUACleaner.doNotRestoreAllCiphersState_supress !== 1)
		{
			HTTPUACleaner.NeedToRestoreAllCiphersState = false;

			var data = HTTPUACleaner['sdk/self'].data;
			HTTPUACleaner.displayNotification(data.url("CipherErrorDialog.html"));

			if (doNotRestore)
				HTTPUACleaner.doNotRestoreAllCiphersState_supress = 1;
		}
		else
			HTTPUACleaner.doNotRestoreAllCiphersState_supress = 2;

		HTTPUACleaner.NeedToRestoreAllCiphersState = false;
	},

	clickOnCipherOptions: function(eventArgs, noSave)
	{
		if (!HTTPUACleaner.httpOptionsInitialized())
		{
			console.error("HUAC has been not initialized (clickOnCipherOptions)");
			return false;
		}

		// Имя настройки шифра
		var cc = eventArgs.cName;
		var st = eventArgs.newState;

		var p = HTTPUACleaner['sdk/preferences/service'];
		var all = p.keys("security.ssl3");
		
		if (st == "+")
		{
			var i = HTTPUACleaner.storage.ciphersOptions.disabled.indexOf(cc);
			if (i >= 0)
				HTTPUACleaner.storage.ciphersOptions.disabled.splice(i, 1);

			if (HTTPUACleaner.storage.ciphersOptions.enabled.indexOf(cc) < 0)
				HTTPUACleaner.storage.ciphersOptions.enabled.push(cc);
		}
		else
		{
			var i = HTTPUACleaner.storage.ciphersOptions.enabled.indexOf(cc);
			if (i >= 0)
				HTTPUACleaner.storage.ciphersOptions.enabled.splice(i, 1);

			if (HTTPUACleaner.storage.ciphersOptions.disabled.indexOf(cc) < 0)
				HTTPUACleaner.storage.ciphersOptions.disabled.push(cc);
		}

		var result = false;
		for (var c in all)
		{
			var cn = all[c];

			if (HTTPUACleaner.isCipher(cc, cn))
			{
				var include = false;
				if (st == "+")
				{
					for (var code in HTTPUACleaner.storage.ciphersOptions.disabled)
					{
						if (HTTPUACleaner.isCipher(HTTPUACleaner.storage.ciphersOptions.disabled[code], cn))
						{
							include = true;
							break;
						}
						
						if (include)
							break;
					}

					if (!include)
					{
						if (p.get(cn) !== true)
						{
							HTTPUACleaner.NoNeedRestoreAllCiphersState = true;
							try
							{
								if (eventArgs.doNotRestore !== true)
									p.set(cn, true);
							}
							finally
							{
								HTTPUACleaner.NoNeedRestoreAllCiphersState = false;
							}
							result = true;
						}
					}
				}
				else
				{
					if (p.get(cn) !== false)
					{
						HTTPUACleaner.NoNeedRestoreAllCiphersState = true;
						try
						{
							if (eventArgs.doNotRestore !== true)
								p.set(cn, false);
						}
						finally
						{
							HTTPUACleaner.NoNeedRestoreAllCiphersState = false;
						}
						result = true;
					}
				}
			}
		}

		if (!HTTPUACleaner.terminated && !noSave)
		{
			HTTPUACleaner.sdb.setHTTP();

			HTTPUACleaner.panel.port.emit
			(
				"updateCiphers",
				HTTPUACleaner.getCiphers()
			);
		}

		return result;
	},

	isCipher: function(cc, cn)
	{
		for (var codei in HTTPUACleaner.ciphersCodes[cc])
		{
			var code = HTTPUACleaner.ciphersCodes[cc][codei];

			var include = true;
			for (var ci in code.i)
			{
				if (cn.indexOf(code.i[ci]) < 0)
				{
					include = false;
					break;
				}
			}
			
			if (!include)
				continue;
			
			for (var ce in code.e)
			{
				if (cn.indexOf(code.e[ce]) >= 0)
				{
					include = false;
					break;
				}
			}
			
			if (!include)
				continue;

			return true;
		}
	},
	
	getCiphers: function()
	{
		if (!HTTPUACleaner.httpOptionsInitialized())
		{
			console.error("HUAC has been not initialized (getCiphers)");
			return false;
		}

		var p = HTTPUACleaner['sdk/preferences/service'];
		var all = p.keys("security.ssl3");
		var EnabledCiphers  = [];
		var DisabledCiphers = [];
		var cnt = 0;
		for (var c in all)
		{
			var cn = all[c];

			for (var cc in HTTPUACleaner.ciphersCodes)
			{
				cnt++;
				if (HTTPUACleaner.isCipher(cc, cn))
				{
					if (p.get(cn))
					{
						if (EnabledCiphers.indexOf(cc) < 0)
							EnabledCiphers.push(cc);
					}
					else
					{
						if (DisabledCiphers.indexOf(cc) < 0)
							DisabledCiphers.push(cc);
					}
				}
			}	
		}

		if (!HTTPUACleaner.storage.ciphersOptions)
		{
			if (!HTTPUACleaner.storage.ciphersOptions)
				HTTPUACleaner.storage.ciphersOptions = {enabled: EnabledCiphers, disabled: [/*"DSS",*/ "MD5", 'RC4']};

			for (var code in DisabledCiphers)
			{
				if (EnabledCiphers.indexOf(DisabledCiphers[code]) < 0)
				{
					HTTPUACleaner.storage.ciphersOptions.disabled.push(DisabledCiphers[code]);
				}
			}
			
			for (var code in EnabledCiphers)
			{
				if (HTTPUACleaner.storage.ciphersOptions.enabled.indexOf(EnabledCiphers[code]) < 0)
				{
					if (/*DisabledCiphers[code] != "DSS" || */DisabledCiphers[code] != "MD5" || DisabledCiphers[code] != "RC4")
						HTTPUACleaner.storage.ciphersOptions.enabled.push(EnabledCiphers[code]);
				}
			}

			if (HTTPUACleaner['sdk/preferences/service'].get(HTTPUACleaner_Prefix + 'debug.writeSettingFilePathes', false))
				console.error(HTTPUACleaner.storage.ciphersOptions);

			HTTPUACleaner.doNotRestoreAllCiphersState_supress = 1;
			HTTPUACleaner.RestoreAllCiphersState();

			HTTPUACleaner.sdb.setHTTP();
		}

		return {
					//all:		all,
					options:	HTTPUACleaner.storage.ciphersOptions,
					enabled:	EnabledCiphers,
					disabled:	DisabledCiphers
				};
	},
	
	getOptions1: function()
	{
		var p = HTTPUACleaner['sdk/preferences/service'];
		var optNames = ["gfx.downloadable_fonts.enabled", "gfx.downloadable_fonts.sanitize", "browser.display.use_document_fonts", "security.ssl.treat_unsafe_negotiation_as_broken", 'network.dns.disablePrefetch', 'network.prefetch-next', 'network.http.speculative-parallel-limit', "security.ssl.require_safe_negotiation", "security.tls.version.min", "security.tls.version.max", "plugin.default.state"/*, "dom.navigator-property.disable.userAgent", "dom.navigator-property.disable.mozId"*/, "security.OCSP.enabled", "security.OCSP.require", "security.xpconnect.plugin.unrestricted", 'security.pki.cert_short_lifetime_in_days', 'webgl.disabled', 'media.peerconnection.enabled'];

		var options = {};
		for (var c in optNames)
		{
			var name = optNames[c];

			var option = {name: name, val: p.get(name)};

			options[name] = option;
		}

		return options;
	},
	
	clickOnOptions1Options: function(eventArgs)
	{
		var cc = eventArgs.cName;
		var st = eventArgs.newState;

		var p = HTTPUACleaner['sdk/preferences/service'];
		// security.tls.version.max	security.tls.version.min
		if (cc.indexOf("security.tls.version.") == 0)
		{
			if (cc.indexOf("security.tls.version.min") == 0)
			{
				var max = p.get("security.tls.version.max");
				var min = st;

				if (min > max)
					min = 0;

				p.set("security.tls.version.min", min);
			}
			else
			{
				var min = p.get("security.tls.version.min");
				var max = st;
				if (max < min)
					max = min;

				p.set("security.tls.version.max", max);
			}
		}
		else
		{
			if (cc == 'network.http.speculative-parallel-limit')
			{
				if (st == '+')
					p.reset(cc, st);
				else
					p.set(cc, st);
			}
			else
			p.set(cc, st);
		}

		if (!HTTPUACleaner.terminated)
		HTTPUACleaner.panel.port.emit
		(
			"updateOptions1",
			HTTPUACleaner.getOptions1()
		);
	},

	getTLSLog: function(options)
	{
		if (options === false)
		{
			if (!HTTPUACleaner.terminated)
			HTTPUACleaner.panel.port.emit
			(
				"setTLSLog",
				''
			);
		}
		else
			if (!HTTPUACleaner.terminated)
			HTTPUACleaner.panel.port.emit
			(
				"setTLSLog",
				HTTPUACleaner.getTLSLogTableObject(HTTPUACleaner.getTLSLogObject(options.TLSLogURL))
			);
	},
	
	getBLog: function(options)
	{
		if (options === false)
		{
			if (!HTTPUACleaner.terminated)
			HTTPUACleaner.panel.port.emit
			(
				"setBLog",
				''
			);
		}
		else
			if (!HTTPUACleaner.terminated)
			HTTPUACleaner.panel.port.emit
			(
				"setBLog",
				HTTPUACleaner.getBLogTableObject(options.BLogURL)
			);
	},

	// options = false - вызов из EndPrivateBrowsing
	getHLog: function(options, worker)
	{
		try
		{
			if (options && options.hlenabled !== undefined && HTTPUACleaner.httplog.enabled != options.hlenabled)
			{
				var prefs = HTTPUACleaner['sdk/preferences/service'];

				HTTPUACleaner.httplog.enabled = !!options.hlenabled;
				prefs.set(HTTPUACleaner_Prefix + 'httplog.enabled', !!options.hlenabled);
			}
		}
		catch (e)
		{
			HTTPUACleaner.logObject(e, true);;
		}

		try
		{
			if (options && options.RESET === true)
			{
				HTTPUACleaner.loggerH.CleanAll();
			}
		}
		catch (e)
		{
			HTTPUACleaner.logObject(e, true);;
		}
		
		if (options.fromContent === true && options.cancelId)
		{
			for (var hn in HTTPUACleaner.LogHeaders)
			{
				lh = HTTPUACleaner.LogHeaders[hn];
				if (lh.subject.channelId == options.cancelId)
				{
					lh.subject.cancel(Cr.NS_BINDING_ABORTED);
					HTTPUACleaner.deleteRequestFromArray();	// чтобы статус запроса сразу же изменился в данных для логирования
					break;
				}
			}
		}

		try
		{
			if (worker === true)
			{
				if (options.fromContent || this.getHLog.tab)
					return;

				//this.getHLog.tab = true;
				HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('httplog.html'), false, 500);
				return;
			}

			try
			{
				if (worker && worker !== true)
					HTTPUACleaner.getHLog.lastWorker = worker;
				else
					worker = HTTPUACleaner.getHLog.lastWorker;

				if (!worker)
				{
					if (options.fromContent || this.getHLog.tab)
						return;

					//this.getHLog.tab = true;
					// Не создаём новых вкладок, если это вызов из EndPrivateBrowsing
					if (options !== false)
					HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('httplog.html'), false, 500);
					return;
				}

				if (options === false)
				{
					if (!HTTPUACleaner.terminated)
					worker.port.emit
					(
						"setHLog",
						{BTable: '', hlenabled: HTTPUACleaner.httplog.enabled}
					);
				}
				else
					if (!HTTPUACleaner.terminated)
					worker.port.emit
					(
						"setHLog",
						{BTable: HTTPUACleaner.getBLogTableObject('', HTTPUACleaner.loggerH, '11', options), hlenabled: HTTPUACleaner.httplog.enabled}
					);
			}
			catch (e)
			{
				// worker бывает выгружен
				if (e.message != "Couldn't find the worker to receive this message. The script may not be initialized yet, or may already have been unloaded.")
					HTTPUACleaner.logObject(e, true);;

				if (options.fromContent || this.getHLog.tab)
					return;

				//this.getHLog.tab = true;
				// Не создаём новых вкладок, если ��то вызов из EndPrivateBrowsing
				if (options !== false)
				HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('httplog.html'), false, 500);
			}
		}
		finally
		{
			this.getHLog.tab = false;
		}
	},
	
	getSideLog: function(options)
	{
		if (options === false)
		{
			if (!HTTPUACleaner.terminated)
			HTTPUACleaner.panel.port.emit
			(
				"setSideLog",
				null
			);
		}
		else
			if (!HTTPUACleaner.terminated)
			{
				HTTPUACleaner.panel.port.emit
					(
						"setSideLog",
						HTTPUACleaner.sdb.getHtmlTab(options.SideLogURL, HTTPUACleaner.sdb.currentSettings, options.scrool, {CurrentHost: options.CurrentHost, CurrentDomain: options.CurrentDomain})
					);
			}
	},
	
	heightDown: function(options)
	{
		if (options.setted < 400 || options.real < 300)
			return;

		var r = (options.real - 32)/(options.setted - 32);

		var p        = HTTPUACleaner['sdk/preferences/service'];
		var font     = p.get(HTTPUACleaner_Prefix + 'mainpanel.font', undefined);
		
		if (font)
			return;
		
		font = '16px "Times New Roman"';
		var fontSize = font.indexOf('px ');
		var fontTail = font.substring(fontSize);
		if (fontSize < 0)
		{
			console.error('no font size in ' + font);
			fontSize = 16;
			fontTail = 'px "Times New Roman"';
		}
		else
			fontSize = Number( font.substring(0, fontSize) );

		if (!!!fontSize)
		{
			console.error('!!!fontSize is true: ' + font);
			return;
		}

		fontSize     = Math.floor(fontSize * r);

		if (!!!fontSize || fontSize < 5)
		{
			console.error('!!!fontSize || fontSize < 5 is true: ' + font);
			return;
		}

		p.set(HTTPUACleaner_Prefix + 'mainpanel.font', fontSize + fontTail);
		console.error('setted panel font ' + font);

		HTTPUACleaner.panel.port.emit
		(
			"heightDownChanged",
			{panel: {font: fontSize + fontTail}}
		);
	},
	
    MainMenu:
	{
		create: function()
		{
			var data = HTTPUACleaner['sdk/self'].data;
			var p    = HTTPUACleaner['sdk/preferences/service'];
			var urls = HTTPUACleaner.urls;
			var tabs = urls.tabs;

			HTTPUACleaner.mainOptionsCaptionsRefresh();
			var dw = 1100;
			var dh = 880;

			HTTPUACleaner.panel = HTTPUACleaner['sdk/panel'].Panel
			({
				contentURL: data.url("popupmenu.html"),
				width: p.get(HTTPUACleaner_Prefix + 'mainpanel.width', dw),	// см. ещё ниже
				height: Number(p.get(HTTPUACleaner_Prefix + 'mainpanel.height', dh)),
				// contentStyle: '* { color: black; font: ' + p.get(HTTPUACleaner_Prefix + 'mainpanel.font', '16px "Times New Roman"') + '}',
				contentScriptFile: [data.url("mainjs/logBView.js"), data.url("popupmenu.js")],
				onShow: function(e)
				{
					if (!HTTPUACleaner.httpOptionsInitialized())
					{
						HTTPUACleaner.panel.hide();

						console.error('HUAC Exclamation: the main panel do not show because HUAC not initialized. Please wait 10-15 seconds (for a green extension icon).');
						return;
					}

					HTTPUACleaner.currentHost = '';
					HTTPUACleaner.currentDomain = '';
					var tlsLogUrl = '';
					var   bLogUrl = '';
					var sideLogUrl = '';
					var lastUrls = {CurrentHost: '', CurrentDomain: ''};
					try
					{
						tlsLogUrl = urls.fromTabDocument(urls.sdkTabToChromeTab(tabs.activeTab));

						if (tlsLogUrl.startsWith('resource://') || tlsLogUrl.startsWith('about:') || tlsLogUrl.startsWith('chrome:') || tlsLogUrl.startsWith('data:') || tlsLogUrl.startsWith('javascript:') || tlsLogUrl.startsWith('blob:'))
							tlsLogUrl = '';

						HTTPUACleaner.currentHost = urls.getHostByURI(tlsLogUrl); //urls.truncateString(urls.getHostByURI(tlsLogUrl), 28);
						HTTPUACleaner.currentDomain = urls.getDomainByHost(HTTPUACleaner.currentHost);
						try
						{
							bLogUrl    = tlsLogUrl;
							sideLogUrl = HTTPUACleaner.currentDomain; //urls.getDomainByHost(HTTPUACleaner.currentHost);
							lastUrls.CurrentHost = HTTPUACleaner.currentHost;
							lastUrls.CurrentDomain = sideLogUrl;
						}
						catch (e)
						{}

						if (!HTTPUACleaner.currentHost && HTTPUACleaner.lastHost)
						{
							HTTPUACleaner.currentHost   = HTTPUACleaner.lastHost; // urls.truncateString(HTTPUACleaner.lastHost, 28);
							HTTPUACleaner.currentDomain = urls.getDomainByHost(HTTPUACleaner.currentHost);
							tlsLogUrl				    = HTTPUACleaner.lastUri;
						}
					}
					catch (e)
					{}

					var r = {NoToBadge: true, f: -1.0};
					HTTPUACleaner.TrustLevelToBadge(r);
					
					var font = p.get(HTTPUACleaner_Prefix + 'mainpanel.font', '16px "Times New Roman"');
					if (font.length < 1)
						font = '16px "Times New Roman"';

					var decodeURIA = function(str, maxLen)
					{
						try
						{
							return urls.truncateString(decodeURI(str), maxLen);
						}
						catch (e)
						{
							return urls.truncateString(str, maxLen);
						}
					};

					if (!HTTPUACleaner.terminated)
					HTTPUACleaner.panel.port.emit
					(
						"init",
						{
							HTTPUACleaner:
							{
								mainOptionsCaptions: 	HTTPUACleaner.mainOptionsCaptions,
								currentHost: 			HTTPUACleaner.currentHost,
								currentDomain: 			HTTPUACleaner.currentDomain,
								currentHostToShow:		decodeURIA(HTTPUACleaner.currentHost, 32),
								currentDomainToShow:	decodeURIA(HTTPUACleaner.currentDomain, 32),
								enabled:				HTTPUACleaner.enabled,
								mainOptionsColors:		HTTPUACleaner.mainOptionsColors,
								mainOptionsNames:		HTTPUACleaner.mainOptionsNames,
								version:				HTTPUACleaner.version
							},
							eo: HTTPUACleaner.enabledOptions,
							ciphers : HTTPUACleaner.getCiphers(),
							options1: HTTPUACleaner.getOptions1(),
							TLS     : {trustLevel: r, tp: HTTPUACleaner.panel.tp, TLSLogURL: tlsLogUrl, loggerSideEnabled: HTTPUACleaner.loggerSide.enabled, sideLogUrl: sideLogUrl, lastUrls: lastUrls, bLogUrl: bLogUrl},
							panel	:
									{
										font:   font,
										height: Number(p.get(HTTPUACleaner_Prefix + 'mainpanel.height', dh))
									}
						}
					);
				},
				onHide: function()
				{
					if (HTTPUACleaner.ToggleButton)
					HTTPUACleaner.ToggleButton          .state('window', {checked: false});

					if (HTTPUACleaner.ToggleButtonTrustLevel)
					HTTPUACleaner.ToggleButtonTrustLevel.state('window', {checked: false});

					// Очищаем панель, чтобы не жрала много памяти просто так. Хотя, кажется, это не сильно на что-то влияет
					if (!HTTPUACleaner.terminated)
					{
						HTTPUACleaner.panel.port.emit
						(
							"setTLSLog",
							''
						);
						
						if (!HTTPUACleaner.terminated)
						HTTPUACleaner.panel.port.emit
						(
							"setBLog",
							''
						);

						HTTPUACleaner.panel.port.emit
						(
							"setSideLog",
							null
						);
					}
				}
			});

			var { ToggleButton } = require('sdk/ui/button/toggle');

			try
			{
				HTTPUACleaner.ToggleButton = ToggleButton
				({
					id: "HTTPUserAgentcleaner",
					label: "HTTP UserAgent cleaner",
					icon:
						{
							"32": "./HTTPUACleaner_notinitialized.png"
						},
					onChange: function(state)
					{
						if (state.checked)
						{
							HTTPUACleaner.panel.tp = 1;
							HTTPUACleaner.panel.show
							({
								position: HTTPUACleaner.ToggleButton,
								width: p.get(HTTPUACleaner_Prefix + 'mainpanel.width', dw),
								height: Number(p.get(HTTPUACleaner_Prefix + 'mainpanel.height', dh)),
								contentStyle: '* { color: black; font: ' + p.get(HTTPUACleaner_Prefix + 'mainpanel.font', '16px "Times New Roman"') + '}'
							});
						}
					}
				});
			}
			catch (e)
			{
				HTTPUACleaner.ToggleButton = null;
				HTTPUACleaner.logObject(e, true);;
			}
			
			try
			{
				HTTPUACleaner.ToggleButtonTrustLevel = ToggleButton
				({
					id: "HTTPUserAgentcleanerTrust",
					label: "HTTP UserAgent cleaner - trust level of site",
					icon:
						{
							"32": "./HTTPUACleaner_trustlevel.png"
						},
					badge: "----",
					badgeColor: "#00AAAA",
					onChange: function(state)
					{
						if (state.checked)
						{
							HTTPUACleaner.panel.tp = 2;
							HTTPUACleaner.panel.show
							({
								position: HTTPUACleaner.ToggleButtonTrustLevel,
								width: p.get(HTTPUACleaner_Prefix + 'mainpanel.width', dw),
								height: Number(p.get(HTTPUACleaner_Prefix + 'mainpanel.height', dh))
							});
						}
					}
				});
			}
			catch (e)
			{
				HTTPUACleaner.ToggleButtonTrustLevel = null;
				HTTPUACleaner.logObject(e, true);;
			}

			tabs.on("open", 	HTTPUACleaner.handleTabOpen);
			tabs.on("ready", 	HTTPUACleaner.handleTabReady);
			tabs.on("activate", HTTPUACleaner.handleTabActivate);
			tabs.on("close",    HTTPUACleaner.handleTabClosing);

			HTTPUACleaner.panel.port.on
			(
				'heightDown',
				HTTPUACleaner.heightDown
			);
			
			HTTPUACleaner.panel.port.on
			(
				'showTLSLog',
				HTTPUACleaner.getTLSLog
			);
			
			HTTPUACleaner.panel.port.on
			(
				'showBLog',
				HTTPUACleaner.getBLog
			);
			
			HTTPUACleaner.panel.port.on
			(
				'showHLog',
				function(opts)
				{
					HTTPUACleaner.getHLog(opts, true)
				}
			);

			HTTPUACleaner.panel.port.on
			(
				'showSideLog',
				HTTPUACleaner.getSideLog
			);
			
			HTTPUACleaner.panel.port.on
			(
				'disableCleaner',
				HTTPUACleaner.disablePlugin
			);

			HTTPUACleaner.panel.port.on
			(
				'enableCleaner',
				HTTPUACleaner.disablePlugin
			);
			
			HTTPUACleaner.panel.port.on
			(
				'clickOnOptions',
				HTTPUACleaner.clickOnOptions
			);
			
			HTTPUACleaner.panel.port.on
			(
				'clickOnPriorityOptions',
				HTTPUACleaner.clickOnPriorityOptions
			);
			
			HTTPUACleaner.panel.port.on
			(
				'clickOnCipherOptions',
				HTTPUACleaner.clickOnCipherOptions
			);
			
			HTTPUACleaner.panel.port.on
			(
				'clickOnOptions1Options',
				HTTPUACleaner.clickOnOptions1Options
			);

			HTTPUACleaner.panel.port.on
			(
				'ResetFF', HTTPUACleaner.resetFireFoxToDefault
			);

			HTTPUACleaner.panel.port.on
			(
				'ResetEx', HTTPUACleaner.resetExtensionToDefault
			);
			
			HTTPUACleaner.panel.port.on
			(
				'Certificates', HTTPUACleaner.certificatesTabOpen
			);
			
			HTTPUACleaner.panel.port.on
			(
				'ResetData', HTTPUACleaner.ResetData
			);
			
			HTTPUACleaner.panel.port.on
			(
				'ResetBLog', HTTPUACleaner.ResetBLog
			);
			
			HTTPUACleaner.panel.port.on
			(
				'ResetSR', HTTPUACleaner.resetSideToDefault
			);

			HTTPUACleaner.panel.port.on
			(
				'ResetSC', HTTPUACleaner.clearSideHistoryToDefault
			);

			HTTPUACleaner.panel.port.on
			(
				'SideTab',
				HTTPUACleaner.SideTabEvent
			);
		}
	},

	displayNotification: function(url, extensionSet)
	{
		//var a = HTTPUACleaner['sdk/window/utils'].openDialog({url: url, features: 'chrome,all,dialog,centerscreen, top'})

		var p = HTTPUACleaner['sdk/preferences/service'];
		if (extensionSet)
		{
			if (p.has(HTTPUACleaner_Prefix + 'display.' + extensionSet) && p.get(HTTPUACleaner_Prefix + 'display.' + extensionSet) === false)
				return;
		}

		var panel = HTTPUACleaner['sdk/panel'].Panel
		({
			width: 550,
			height: url.indexOf('CipherErrorDialog') > 0 ? 220 : 400,
			contentURL: url,
			contentScriptFile: HTTPUACleaner['sdk/self'].data.url('CipherDialog.js'),
			onShow: function()
			{
				if (!HTTPUACleaner.terminated)
				panel.port.emit
				(
					"initCipherSettingsDialog",
					{
						extensionSet: extensionSet,
						strings:      {OK: "OK", NoMoreDialogs: HTTPUACleaner['sdk/l10n'].get("No more dialogs")}
					}
				);
				
			}
		});
		panel.port.on
			(
				'SetNoDialog',
				function()
				{
					if (extensionSet)
						p.set(HTTPUACleaner_Prefix + 'display.' + extensionSet, false);
				}
			);
		panel.port.on
			(
				'CloseDialog',
				function()
				{
					panel.hide();
				}
			);

		panel.show();
	},

	setOtherExtensionsForWell: function()
	{
		var t1 = 0; var t2 = 0; var t = 0;
		var p = HTTPUACleaner['sdk/preferences/service'];
		if (
			   p.has('noscript.doNotTrack.enabled')
			&& p.get("noscript.doNotTrack.enabled") !== false
			)
		{
			if (!p.has(HTTPUACleaner_Prefix + 'noscript.doNotTrack.enabled'))
				 p.set(HTTPUACleaner_Prefix + 'noscript.doNotTrack.enabled', p.get("noscript.doNotTrack.enabled"));

			p.set("noscript.doNotTrack.enabled", false);
			
			t++;
			t1++;
			
		}

		if (t > 0)
		{
			var data = HTTPUACleaner['sdk/self'].data;
			HTTPUACleaner.displayNotification(data.url("TerribleDialogNS.html"), "NoScriptDNT");
			/*
			if (t == 1)
			{
				if (t1 > 0)
					HTTPUACleaner.displayNotification(data.url("TerribleDialogNS.html"), "NoScriptDNT");
				else
					HTTPUACleaner.displayNotification(data.url("TerribleDialogC.html"), "CipherFoxRC4");
			}
			else
				HTTPUACleaner.displayNotification(data.url("TerribleDialogA.html"), "CipherFoxRC4_NoScriptDNT");*/
		}
	},
	
	resetFireFoxToDefault: function()
	{	
		HTTPUACleaner.NeedToRestoreAllCiphersState = true;
		HTTPUACleaner.doNotRestoreAllCiphersState_supress = 1;

		var p = HTTPUACleaner['sdk/preferences/service'];

		try
		{
			var all = p.keys("security.ssl3");
			for (var c in all)
			{
				var name = all[c];
				p.reset(name);
			}
		}
		catch (e)
		{
			HTTPUACleaner.logObject(e, true);;
		}
		
		var names = ["gfx.downloadable_fonts.enabled", 
			"gfx.downloadable_fonts.sanitize", 
			"browser.display.use_document_fonts", 
			"security.ssl.treat_unsafe_negotiation_as_broken", 
			"security.ssl.require_safe_negotiation", 
			"security.tls.version.min", 
			"security.tls.version.max", 
			"plugin.default.state", 
			"security.xpconnect.plugin.unrestricted",
			"security.OCSP.enabled", 
			"security.OCSP.require"];

		for (var i in names)
			p.reset(names[i]);

		delete HTTPUACleaner.storage.ciphersOptions;
		HTTPUACleaner.sdb.setHTTP();

		p.reset(HTTPUACleaner_Prefix + 'mainpanel.font');
		p.reset(HTTPUACleaner_Prefix + 'mainpanel.width');
		p.reset(HTTPUACleaner_Prefix + 'mainpanel.height');
	},

	resetExtensionToDefault: function()
	{
		delete HTTPUACleaner.storage.enabledOptions;
		HTTPUACleaner.sdb.setHTTP();
	},

	certificatesTabOpen: function(uri)
	{
		HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('certs/main.html'));

		//HTTPUACleaner.certsObject.showAll();
	},

	lastFailureTabOpen: 0,
	lastFailureTabOpenL: 0,
	certificatesFailureTabOpen: function(noTimeout)
	{
		if (!noTimeout && Date.now() - HTTPUACleaner.lastFailureTabOpen < 10*1000)
		HTTPUACleaner.timers.setTimeout
		(
			function()
			{
				// Если вкладка была отк��ыта менее, чем 10 секунд назад, значит счита��м, что она уже была открыта другим запросом во время ож��дания этого запр��са
				// И не открываем её
				
				if (Date.now() - HTTPUACleaner.lastFailureTabOpen < 10*1000)
				{
					if (!this.options || !this.options.certsFailure || HTTPUACleaner.lastFailureTabOpenL == this.options.certsFailure.length)
						return;

					HTTPUACleaner.certificatesFailureTabOpen(noTimeout);
					return;
				}


				var noR = HTTPUACleaner.certsObject.showAllFailures();
				HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('certs/failure.html'), noR, noR ? 1000 : 3000);

				HTTPUACleaner.lastFailureTabOpen = Date.now();
				HTTPUACleaner.lastFailureTabOpenL = this.options && this.options.certsFailure ? this.options.certsFailure.length : 0;
			},
			10*1000 - (Date.now() - HTTPUACleaner.lastFailureTabOpen) + 1000
		);
		else
		{
			HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('certs/failure.html'), HTTPUACleaner.certsObject.showAllFailures(), noTimeout ? 200 : 5000);

			HTTPUACleaner.lastFailureTabOpen = Date.now();
			HTTPUACleaner.lastFailureTabOpenL = this.options && this.options.certsFailure ? this.options.certsFailure.length : 0;
		}
	},

	hostsFailureTabOpen: function()
	{
		HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('certs/hosts.html'));
		//HTTPUACleaner.certsObject.showAllFailures();
	},

	tabOpen: function(uri, noReload, timeoutToActivate, onTerminated)
	{
		if (HTTPUACleaner.terminated && onTerminated !== true)
			return;

		var urls = HTTPUACleaner.urls; //require('./getURL');
		var tabs = urls.tabs;

		var found = false;
		for (var tabIndex in tabs)
		{
			try
			{
				var tab = tabs[tabIndex];
				var taburl = HTTPUACleaner.urls.fromTabDocument(HTTPUACleaner.urls.sdkTabToChromeTab(tab)).split('#')[0];
				if (uri == taburl)
				{
					found = true;
					if (!noReload)
						tab.reload();

					if (!noReload || timeoutToActivate)
						HTTPUACleaner.timers.setTimeout
						(
							function()
							{
								tab.activate();
							},
							timeoutToActivate ? timeoutToActivate : 200
						);
					else
						tab.activate();

					break;
				}
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
			}
		}

		if (HTTPUACleaner && HTTPUACleaner.panel)
		HTTPUACleaner.panel.hide();

		if (found)
		{
			return true;
		}

		tabs.open
		({
			url: uri
		});

		return false;
	},

	ResetData: function()
	{
		HTTPUACleaner.endPrivateBrowsing();
	},
	
	ResetBLog: function()
	{
		HTTPUACleaner.loggerB.CleanAll();
		//HTTPUACleaner.loggerH.CleanAll();
		HTTPUACleaner.getBLog(false);
		//HTTPUACleaner.getHLog(false);
	},

	resetSideToDefault: function()
	{
		HTTPUACleaner.clearSideHistoryToDefault();
	},

	clearSideHistoryToDefault: function()
	{
	},

	calcTrustLevelColor: function(f, badge, noHigh, blue)
	{
		var h = noHigh ? 0.8 : 0.5;
		if (Number.isFinite(noHigh))
			h = noHigh;

		var clr = f < h ? 0xFF : Math.floor((1.0-f)/(1.0-h)*0xFF);
		var clg = f >= h ? 0xFF : Math.floor(0xFF + (f-h)/h*0xFF);

		if (badge)
		{
			clr -= clr >> 2;
			clg -= clg >> 2;
		}

		var cl  = clg << 8;
		if (blue)
			cl += clr;
		else
			cl += clr << 16;

		cl  = cl.toString(16);
		while (cl.length < 6)
			cl = '0' + cl;

		var fa = (f*100.0).toString().substring(0, 4).split('.')[0];


		if (f < 0.01)
			fa = '0';

		if (blue && clr > 0x80)
		{
			return [fa, cl, 'FFFFFF'];
		}

		return [fa, cl, '000000'];
	},
	
	ShowTrustLevelToBadge: function(f, fh, result)
	{
		var a  = HTTPUACleaner.calcTrustLevelColor(f,  !result.NoToBadge);
		var ah = HTTPUACleaner.calcTrustLevelColor(fh, !result.NoToBadge);

		if (!result || result.NoToBadge !== true)
		{
			if (HTTPUACleaner.ToggleButtonTrustLevel)
			{
				HTTPUACleaner.ToggleButtonTrustLevel.state(HTTPUACleaner.ToggleButtonTrustLevel, {badge: a[0], badgeColor: '#' + a[1]});
				if (fh < 0.2)
					HTTPUACleaner.ToggleButtonTrustLevel.state(HTTPUACleaner.ToggleButtonTrustLevel, {icon: {'32': "./HTTPUACleaner_trustlevel0.png"}});
				else
				if (fh < 0.5)
					HTTPUACleaner.ToggleButtonTrustLevel.state(HTTPUACleaner.ToggleButtonTrustLevel, {icon: {'32': "./HTTPUACleaner_trustlevel2.png"}});
				else
				if (fh < 0.75)
					HTTPUACleaner.ToggleButtonTrustLevel.state(HTTPUACleaner.ToggleButtonTrustLevel, {icon: {'32': "./HTTPUACleaner_trustlevel5.png"}});
				else
				if (fh < 0.95)
					HTTPUACleaner.ToggleButtonTrustLevel.state(HTTPUACleaner.ToggleButtonTrustLevel, {icon: {'32': "./HTTPUACleaner_trustlevel75.png"}});
				else
					HTTPUACleaner.ToggleButtonTrustLevel.state(HTTPUACleaner.ToggleButtonTrustLevel, {icon: {'32': "./HTTPUACleaner_trustlevel1.png"}});
			}

			if (HTTPUACleaner.ToggleButton)
				HTTPUACleaner.ToggleButton.state(HTTPUACleaner.ToggleButton, {badge: ah[0], badgeColor: '#' + ah[1]});
		}

		if (result)
		{
			result.f  = f;
			result.cl = '#' + a[1];
		}
	},

	TrustLevelToBadgeTabRequests: function(result)
	{
		var urls = HTTPUACleaner.urls;

		var tabInfo = HTTPUACleaner.logger.FindTab(urls.fromTabDocument(urls.sdkTabToChromeTab(urls.tabs.activeTab)));
		if (tabInfo == null)
			return false;
	
		if (!result)
			result = {};

		var TLSInfo = tabInfo.TLSInfo;
		
		var f  = 1.0;
		var fl = 1.0;
		var fa = 1.0;
		var fh = 1.0;
		var faFound = false;
		var fhFound = false;
		for (var ti in TLSInfo)
		{
			var t = TLSInfo[ti];
			if (f > t.f)
			{
				f  = t.f;
			}
			if (fa > t.f && t.f > 0 && t.f*100 > HTTPUACleaner.minTLSStrong)
			{
				fa = t.f;
			}

			if (t.fHPKP)
			{
				fhFound = true;
			}

			if (t.fHPKP !== false && fh > t.fHPKP)
			{
				fh = t.fHPKP;
			}

			if (t.f > 0 && t.f*100 > HTTPUACleaner.minTLSStrong)
				faFound = true;
			
			if (fl > t.flong)
				fl = t.flong;
		}

		if (!faFound)
		{
			throw new Error('HTTP UserAgent Cleaner: no tls requests');
		}
		
		if (!fhFound)
			fh = 0.0;

		result.fa = faFound ? fa : 0.0;
		HTTPUACleaner.ShowTrustLevelToBadge(f, fh, result);

		result.long = {NoToBadge: true};
		HTTPUACleaner.ShowTrustLevelToBadge(fl, 0.0, result.long);

		//	console.error(HTTPUACleaner.logger.tabs);
		
		return true;
	},
	
	TrustLevelToBadge: function(result)
	{
		if (!result)
			result = {};
		
		if (HTTPUACleaner.EstimateTLS)
		try
		{
			//var si      = HTTPUACleaner.logger.getSecurityInfoForBrowser(require("sdk/window/utils").getMostRecentBrowserWindow().gBrowser.selectedBrowser);

			if (HTTPUACleaner.TrustLevelToBadgeTabRequests(result) === true)
				return;
		}
		catch (e)
		{/*
			if (HTTPUACleaner.debug)
				HTTPUACleaner.logObject(e, true);;*/
		}

		if (HTTPUACleaner.ToggleButtonTrustLevel)
		{
			HTTPUACleaner.ToggleButtonTrustLevel.state(HTTPUACleaner.ToggleButtonTrustLevel, {badge: '----', badgeColor: "#00CCCC"});
			HTTPUACleaner.ToggleButtonTrustLevel.state(HTTPUACleaner.ToggleButtonTrustLevel, {icon: {'32': "./HTTPUACleaner_trustlevel.png"}});
		}

		if (HTTPUACleaner.ToggleButton)
			HTTPUACleaner.ToggleButton.state(HTTPUACleaner.ToggleButton, {badge: '', badgeColor: ''});

		if (result)
		{
			result.f       = -1.0;
			result.cl      = '#00CCCC';
			result.long    = {};
			result.long.f  = -1.0;
			result.long.cl = '#00CCCC';
		}
	},

	tabsListener:
	{
		lastUrl: null,
		lastUrls: []
	},

	asyncMessages: function(obj)
	{
		if (obj.data.msg == 'loggerB')
		{
			try
			{
				HTTPUACleaner.loggerB.addOToLog(obj.data.val);
			}
			catch(e)
			{
				HTTPUACleaner.logObject(e, true);;
			}
		}
		else
		{
			console.error('Http UserAgent Cleaner: Not found returnMsg name');
			console.error(obj.data);
		}
	},
	
	syncMessages: function(obj)
	{
		if (obj.data.msg == 'CP')
		{
			try
			{
				return HTTPUACleaner.observer.shouldLoad.apply(HTTPUACleaner.observer, obj.data.args);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);;
				return Ci.nsIContentPolicy.REJECT_REQUEST;
			}
		}
		else
		{
			console.error('Http UserAgent Cleaner: Not found returnMsg name');
			console.error(obj.data);
		}
		
		return Ci.nsIContentPolicy.REJECT_REQUEST;
	},
	
	registerBrowserBySdkTab: function(tab, register)
	{
	},
	
	handleTabOpen: function(tab)
	{
		HTTPUACleaner.registerBrowserBySdkTab(tab);
		HTTPUACleaner.handleTabReady(tab);
	},
	
	handleTabReady: function(tab)
	{
		if (HTTPUACleaner.NeedToRestoreAllCiphersState === true)
		{
			HTTPUACleaner.RestoreAllCiphersState();
		}

		HTTPUACleaner.logger    .CleanLog();
		// HTTPUACleaner.loggerSide.CleanLog();

		// При этом удаляются логи служебных запросов, которые залогированы на неверные адреса вкладок. Это может вызвать проблемы при отладке
		HTTPUACleaner.loggerB   .CleanLog();

		HTTPUACleaner.TrustLevelToBadge();
	},
	
	handleTabActivate: function(tab)
	{
		HTTPUACleaner.TrustLevelToBadge();
	},
	
	handleTabClosing: function(tab)
	{
		HTTPUACleaner.clearOutdatedHostOptions(false);
		HTTPUACleaner.deleteRequestFromArray();
		HTTPUACleaner.logger    .CleanLog();
		HTTPUACleaner.loggerB   .CleanLog();
		HTTPUACleaner.loggerSide.CleanLog();
	},

	endPrivateBrowsing: function()
	{
		HTTPUACleaner.logger.resetCachedDataByTime(true);

		HTTPUACleaner.clearOutdatedHostOptions(true);
		HTTPUACleaner.deleteRequestFromArray();
		
		HTTPUACleaner.currentHost = '';
		HTTPUACleaner.lastHost    = '';

		HTTPUACleaner.setCookieRandomStr();

		HTTPUACleaner.logger    .CleanAll(true);
		HTTPUACleaner.loggerB   .CleanAll();
		HTTPUACleaner.loggerH.CleanAll();
		//HTTPUACleaner.loggerSide.CleanAll(true);
		HTTPUACleaner.getTLSLog(false);
		HTTPUACleaner.getBLog  (false);
		HTTPUACleaner.getHLog  (false);

		HTTPUACleaner.certsObject.unknownHosts = {};

		// HTTPUACleaner.clearSideHistoryToDefault();
	},

	prefsObservers: {},
	
	addPrefsObserver: function(add)
	{
		var { PrefsTarget } = require("sdk/preferences/event-target");
		var prefsObservers  = HTTPUACleaner.prefsObservers;
		
		var t1;
		var t2;
		var t3;
		var t4;
		var t5;
		if (!prefsObservers['t1'])
		{
			// Если создать ещё раз, то, почему-то, потом не работает simple-storage
			t1 = PrefsTarget({branchName: 'noscript.doNotTrack.'});
			t2 = PrefsTarget({branchName: 'security.ssl3'});
			t3 = PrefsTarget({branchName: HTTPUACleaner_Prefix + ''});
			t4 = PrefsTarget({branchName: 'network'});
			t5 = PrefsTarget({branchName: 'browser.tabs.remote.'});

			prefsObservers['t1'] = t1;
			prefsObservers['t2'] = t2;
			prefsObservers['t3'] = t3;
			prefsObservers['t4'] = t4;
			prefsObservers['t5'] = t5;
		}
		else
		{
			t1 = prefsObservers['t1'];
			t2 = prefsObservers['t2'];
			t3 = prefsObservers['t3'];
			t4 = prefsObservers['t4'];
			t5 = prefsObservers['t5'];
		}

		if (add)
		{
			//HTTPUACleaner.prefService.addObserver('noscript.doNotTrack.', 	this, false);
			//HTTPUACleaner.prefService.addObserver('security.ssl3', 			this, false);

			prefsObservers['noscript.doNotTrack.'] 			 = this.observe('noscript.doNotTrack.');
			prefsObservers['security.ssl3'] 				 = this.observe('security.ssl3');
			prefsObservers[HTTPUACleaner_Prefix + ''] = this.observe(HTTPUACleaner_Prefix + '');
			prefsObservers['network'] 				 = this.observe('network');
			prefsObservers['browser.tabs.remote.'] 	 = this.observeE10s('browser.tabs.remote.');

			t1.on('', prefsObservers['noscript.doNotTrack.']);
			t2.on('', prefsObservers['security.ssl3']);
			t3.on('', prefsObservers[HTTPUACleaner_Prefix + '']);
			t4.on('', prefsObservers['network']);
			t5.on('', prefsObservers['browser.tabs.remote.']);
		}
		else
		{
			t1.removeListener('', prefsObservers['noscript.doNotTrack.']);
			t2.removeListener('', prefsObservers['security.ssl3']);
			t3.removeListener('', prefsObservers[HTTPUACleaner_Prefix + '']);
			t4.removeListener('', prefsObservers['network']);
			t5.removeListener('', prefsObservers['browser.tabs.remote.']);
		}
	},

	observe: function(branch)
	{
		return function(subject, topic, data)
		{
			var ffpod = HTTPUACleaner.FFPrefsObserverDebug;
			var prefs = HTTPUACleaner['sdk/preferences/service'];
			//if (HTTPUACleaner.debug)
			{
				HTTPUACleaner.FFPrefsObserverDebug = prefs.get(HTTPUACleaner_Prefix + 'debug.FFPrefsObserver',       false);
			}

			if (
				   branch + subject == 'network.tcp.keepalive.enabled'
				|| branch + subject == 'network.http.tcp_keepalive.short_lived_connections'
				|| branch + subject == 'network.http.tcp_keepalive.long_lived_connections'
				|| branch + subject == 'network.tcp.keepalive.idle_time'
				|| branch + subject == 'network.http.keep-alive.timeout'
				|| branch + subject == 'network.http.tcp_keepalive.long_lived_idle_time'
				|| branch + subject == 'network.http.tcp_keepalive.short_lived_idle_time'
				|| branch + subject == 'network.http.tcp_keepalive.short_lived_time'
			)
			{/*
				if (HTTPUACleaner.certsObject.hostsOpt.autodisable && prefs.get('network.tcp.keepalive.enabled'))
				{
					var notifications = HTTPUACleaner.notifications;
					notifications.notify
					({
						title: 		HTTPUACleaner['sdk/l10n'].get('HTTP UserAgent Cleaner extension CONFLICT!'),
						text: 		HTTPUACleaner['sdk/l10n'].get('HTTP UserAgent Cleaner extension detected control of the "network.tcp.keepalive.enabled" option state what out of this extension. This will conflict for "Autodisable root certificate" option and will occur error.'),
						iconURL: 	HTTPUACleaner['sdk/self'].data.url("HTTPUACleaner.png"),
						data: 		"" + HTTPUACleaner.notificationNumber,
						onClick: 	function() {}
					});
				}*/
				
				HTTPUACleaner.certsObject.keepAliveOptionChanged();
			}

			HTTPUACleaner.setPreferences();

			if (subject.indexOf('nonPrivateCookieIsolation') >= 0)
				HTTPUACleaner.setCookieRandomStr(true);

			if (HTTPUACleaner.FFPrefsObserverDebug || ffpod)
			{
				//console.error(subject); // '.ecdhe_ecdsa_aes_128_gcm_sha256'

				var logMessage = HTTPUACleaner.logMessage;
				//var logObject  = HTTPUACleaner.logObject;

				var rmsg = '';
				rmsg += "HTTP UserAgent Cleaner: preference changed";
				rmsg += "\r\n";
				rmsg += subject;
				rmsg += "\r\n";
				rmsg += topic;
				rmsg += "\r\n";
				rmsg += data;
				rmsg += "\r\n";
				rmsg += branch + subject + ' = ';
				try
				{
					rmsg += '' + prefs.get(branch + subject);
				}
				catch (e)
				{}
				rmsg += "\r\n";

				var c = components.stack.caller;

				// console.error(c.formattedStack);
				while (c && c.filename)
				{
					rmsg += c.filename + ":" + c.lineNumber + "\r\n";
					c = c.caller;
				}
				rmsg += "\r\n";
				rmsg += "end of change information of " + branch + subject;
				rmsg += "\r\n";

				logMessage(rmsg);
			}

			if (!HTTPUACleaner.NeedToRestoreAllCiphersState)
			{
				HTTPUACleaner.RestoreAllCiphersState(true);			// это просто сообщение пользователю о конфликте
				HTTPUACleaner.NeedToRestoreAllCiphersState = true;	// вызываем дополнительно для того, чтобы привести настройки в правильный вид
			}
		}
	},
	
	observeE10s: function()
	{
		return function(subject, topic, data)
		{
			HTTPUACleaner.setE10S();
		};
	},

	setE10S: function()
	{
		var prefs = HTTPUACleaner['sdk/preferences/service'];

		var urls = HTTPUACleaner.urls;
		HTTPUACleaner.isE10S  = urls.isE10S;
		HTTPUACleaner.isE10SA = prefs.get('browser.tabs.remote.autostart') || prefs.get('browser.tabs.remote.autostart.1') || prefs.get('browser.tabs.remote.autostart.2');

		if (HTTPUACleaner.isE10SA && !HTTPUACleaner.setE10S_informed)
		{
			HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('e10s.html'), undefined, undefined, true);
			HTTPUACleaner.setE10S_informed = true;
		}

		HTTPUACleaner.disableE10S();
	}
};

HTTPUACleaner.errorLoad = false;

const consoleJSM = HTTPUACleaner.fxVersion44 >= 0 ? Cu.import("resource://gre/modules/Console.jsm", {}) : Cu.import("resource://gre/modules/devtools/Console.jsm", {});

try
{
	consoleJSM.console.logp = function (args, privArgs, notLog)
	{
		if (notLog === true)
			consoleJSM.console.log(privArgs);
		else
			consoleJSM.console.log(args);
	};


	HTTPUACleaner['sdk/preferences/service'] = require('sdk/preferences/service');
	HTTPUACleaner['sdk/l10n'] = require('sdk/l10n');
	HTTPUACleaner.urls = require('./getURL');
	HTTPUACleaner['sdk/private-browsing'] = require("sdk/private-browsing");
	HTTPUACleaner['sdk/window/utils'] = require("sdk/window/utils");
	HTTPUACleaner['sdk/tabs/utils'] = require("sdk/tabs/utils");
	HTTPUACleaner['sdk/self'] = require("sdk/self");

	HTTPUACleaner.loader = Cc["@mozilla.org/moz/jssubscript-loader;1"].getService(Ci.mozIJSSubScriptLoader);
	HTTPUACleaner.tloader = require('toolkit/loader').Loader;

	HTTPUACleaner.loadScript = function(scriptName)
	{
		var data = HTTPUACleaner['sdk/self'].data;
		HTTPUACleaner.loader.loadSubScript(data.url(scriptName));
	};

	HTTPUACleaner.loadScript('utils/mutex.js');
	HTTPUACleaner.loadScript('utils/browsers.js');

	HTTPUACleaner.setE10S();

	HTTPUACleaner.loadScript('mainjs/main_setobserver.js');

	HTTPUACleaner.loadScript('mainjs/getTLSLogTableObject.js');
	HTTPUACleaner.loadScript('mainjs/getBLogTableObject.js');
	HTTPUACleaner.loadScript('mainjs/settingsDb.js');
	HTTPUACleaner.loadScript('mainjs/settingsDbSettings.js');
	HTTPUACleaner.loadScript('mainjs/settingsDbCheck.js');
	HTTPUACleaner.loadScript('mainjs/settingsDbHttp.js');

	HTTPUACleaner.loadScript('mainjs/settingsRule.js');
	HTTPUACleaner.loadScript('mainjs/settingsHtmlTab.js');
	HTTPUACleaner.loadScript('certs/main.js');
	HTTPUACleaner.loadScript('certs/mainTable.js');
	HTTPUACleaner.loadScript('certs/hostsTable.js');
	HTTPUACleaner.loadScript('certs/failuresTable.js');
}
catch (e)
{
	HTTPUACleaner.logMessage('startup error (loadScript)', true);
	HTTPUACleaner.logObject(e, true, true);

	// throw e;
	HTTPUACleaner.errorLoad = true;
}

try
{
	HTTPUACleaner.certsObject = new HTTPUACleaner.certs('HttpUserAgentCleanerCerts.opt');


	HTTPUACleaner.sdb = new HTTPUACleaner.sdbP
	(
		'HttpUserAgentCleaner.opt',
		(function (HTTPUACleaner)
		{
			return function(result)
			{
				if (!result)
				{
					console.error('HUAC: Fatal error! Disable extension and mail to developer');
					HTTPUACleaner.logMessage('startup error (new HTTPUACleaner.sdbP)');

					return;
				}

				HTTPUACleaner.sdb.loadSettings
				(
					function(str)
					{
						if (this.startupError === true)
							return;

						// Создаёт опции по умолчанию, если их нет
						try
						{
							HTTPUACleaner.sdb.getOptions();

							if (HTTPUACleaner.allBlock !== false)
							{
								HTTPUACleaner.sdb.startUpContinue();

								HTTPUACleaner.allBlock++;
								HTTPUACleaner.setPluginButtonState();
							}
						}
						catch (e)
						{
							HTTPUACleaner.logMessage('startup error (HTTPUACleaner.sdb.loadSettings)');
							HTTPUACleaner.logObject(e, true);
						}

						HTTPUACleaner.sdb.setHttpAllowed.release();
					}
				);
			};
		})(HTTPUACleaner)
	);
}
catch (e)
{
	HTTPUACleaner.logMessage('startup error (new HTTPUACleaner.sdbP or new HTTPUACleaner.certs)', true);
	HTTPUACleaner.logObject(e, true);
	
	//throw e;
	HTTPUACleaner.errorLoad = true;
}

try
{
	HTTPUACleaner.logger = require('./logTLS').logger;
	HTTPUACleaner.logger = new HTTPUACleaner.logger();

	HTTPUACleaner.loggerB = require('./logB').logger;
	HTTPUACleaner.loggerH = new HTTPUACleaner.loggerB();
	HTTPUACleaner.loggerB = new HTTPUACleaner.loggerB();

	HTTPUACleaner.loggerSide = require('./logSide').logger;
	HTTPUACleaner.loggerSide = new HTTPUACleaner.loggerSide();

	HTTPUACleaner.SideTabEvent = function(data)
	{
		HTTPUACleaner.sdb.event(data);
	};

	HTTPUACleaner.sdb.response = function(data)
	{
		if (HTTPUACleaner.panel)
		HTTPUACleaner.panel.port.emit
		(
			"SideTableContent",
			data
		);
	};

	HTTPUACleaner.setCookieRandomStr();
}
catch (e)
{
	HTTPUACleaner.logMessage('startup error (logTLS)', true);
	HTTPUACleaner.logObject(e, true);
	
	//throw e;
	HTTPUACleaner.errorLoad = true;
}

exports.onUnload = function (reason)
{
	try
	{
		
		//var HTTPUACleaner = exports.HTTPUACleaner;
		if (HTTPUACleaner)
		{
			//console.error('HTTPUACleaner.onUnload with reason ' + reason);
		}
		else
		{
			/*console.error('HTTPUACleaner.onUnload not have HTTPUACleaner with reason ' + reason);
			console.error('HTTPUACleaner.onUnload skipped');*/
			return;
		}

		HTTPUACleaner.enabled = false;

		//if (reason == "shutdown")
		HTTPUACleaner.shutdown(reason == "uninstall" || reason == "disable");

		if (reason == "uninstall" || /*reason == "shutdown" || */reason == "disable")
		{
			var p = require('sdk/preferences/service');
			if (p.has('noscript.doNotTrack.enabled') && p.has(HTTPUACleaner_Prefix + "noscript.doNotTrack.enabled"))
			{
				p.set("noscript.doNotTrack.enabled", p.get(HTTPUACleaner_Prefix + 'noscript.doNotTrack.enabled'));
				p.reset(HTTPUACleaner_Prefix + "noscript.doNotTrack.enabled");
			}

			if (reason == "uninstall")
			{
				HTTPUACleaner.resetExtensionToDefault();
				HTTPUACleaner.resetFireFoxToDefault();
			}
		}
	}
	catch (e)
	{
		if (HTTPUACleaner)
		{
			HTTPUACleaner.logMessage('shutdown exception', true);
			HTTPUACleaner.logObject(e, true);
		}
		else
			console.error(e);
	}

	HTTPUACleaner = undefined;
};

// options.loadReason игнорируем, т.к. плагин всё равно запускается только один раз, по любой причине
exports.main = function (options, callbacks)
{
	if (HTTPUACleaner.errorLoad)
	{
		console.error("HUAC FATAL ERROR: error occurred during the extension startup");

		try	
		{
			HTTPUACleaner.terminated = false;
			HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('startuperror.html'), undefined, undefined, true);
		}
		catch (e)
		{}

		HTTPUACleaner.terminated = true;
		HTTPUACleaner.logMessage('error shutdown on startup (HTTPUACleaner.errorLoad)', true);

		return;
	}

	if (HTTPUACleaner.fxVersion53 >= 0)
	{
		HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('53error.html'), undefined, undefined, true);
	}

	try
	{
		HTTPUACleaner.startup();
	}
	catch (e)
	{
		HTTPUACleaner.logMessage('startup error');
		HTTPUACleaner.logObject(e, true);
		
		try	
		{
			HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('startuperror.html'), undefined, undefined, true);
		}
		catch (e)
		{}

		HTTPUACleaner.terminated = true;

		throw e;
	}
};

if (!HTTPUACleaner.errorLoad)
try
{
	if (HTTPUACleaner.allBlock !== false)
	{
		HTTPUACleaner.allBlock++;
		HTTPUACleaner.setPluginButtonState();
	}
}
catch (e)
{
	HTTPUACleaner.logMessage('startup error (setPluginButtonState)');
	HTTPUACleaner.logObject(e, true);

	HTTPUACleaner.errorLoad = true;
}
