HTTPUACleaner.notTitle = new Object();
				// https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/nsIContentPolicy
				// http://dxr.mozilla.org/mozilla-central/source/dom/base/nsIContentPolicyBase.idl

// Названия до пробела не изменять! От них зависит фильтрация по ft.
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_DOCUMENT]				= 'page';
if (Ci.nsIContentPolicy.TYPE_SUBDOCUMENT)
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_SUBDOCUMENT] 			= 'frame';	// in FF 42 gone
if (Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME)
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME] 		= 'frame';
if (Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME)
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME] 		= 'frame';	// iframe - изменение повлияет на обработку правил FRAME
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_OTHER] 				= 'other';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_OBJECT] 				= 'object';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_SCRIPT] 				= 'script';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_IMAGE] 				= 'image';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_STYLESHEET] 			= 'CSS';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_PING] 					= 'ping';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_XBL] 					= 'XBL';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_DTD] 					= 'DTD';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_XMLHTTPREQUEST] 	  	= 'ajax';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_OBJECT_SUBREQUEST] 	= 'subrequest';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_FONT] 					= 'font';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_MEDIA] 				= 'media';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_WEBSOCKET] 			= 'websocket';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_CSP_REPORT] 			= 'CSPR: content security policy report';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_XSLT] 					= 'XSLT';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_BEACON] 				= 'beacon (navigator.sendBeacon) message';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_FETCH] 				= 'fetch request';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_IMAGESET] 				= 'imageset (<picture>) request';
HTTPUACleaner.notTitle["" + Ci.nsIContentPolicy.TYPE_WEB_MANIFEST] 			= 'WM: web manifest';

/*
console.error('1: ' + Ci.nsIContentPolicy.TYPE_SUBDOCUMENT);
console.error('2: ' + Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME);
console.error('3: ' + Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME);

if (!Ci.nsIContentPolicy.TYPE_SUBDOCUMENT)
	Ci.nsIContentPolicy.TYPE_SUBDOCUMENT = Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME;
if (!Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME)
	Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME = Ci.nsIContentPolicy.TYPE_SUBDOCUMENT;
if (!Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME)
	Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME = Ci.nsIContentPolicy.TYPE_SUBDOCUMENT;

console.error('1: ' + Ci.nsIContentPolicy.TYPE_SUBDOCUMENT);
console.error('2: ' + Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME);
console.error('3: ' + Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME);
*/

HTTPUACleaner.isNonBlockHttpsFilter = function()
{
	// forceHttps - принудительно перенаправлять на https
	return HTTPUACleaner['sdk/preferences/service'].get(HTTPUACleaner_Prefix + 'forceHttps', false);
};

HTTPUACleaner.setObserver = function()
{
	var observer = function() {};
	observer.prototype = 
	{
		/**
		* nsIObserver
		*/
		
		suspended: [],
		
		observe: function(subjectA/*, topic, data*/) 
		{
			if (!HTTPUACleaner.enabled/* || HTTPUACleaner.terminated*/)
			{
				return;
			}

			// subjectA == undefined. Это холостой вызов для инициализации запросов, которые ранее были приостановлены
			if (!subjectA && HTTPUACleaner.allBlock != 3)
				return;

			var topic 	= subjectA ? subjectA.type		: undefined;
			var subject = subjectA ? subjectA.subject	: undefined;
			var data	= subjectA ? subjectA.data		: undefined;

			var path = "";

			try
			{
				var httpChannel = subject ? subject.QueryInterface(Ci.nsIHttpChannel) : undefined;
				path = httpChannel ? httpChannel.URI.spec : undefined;

				if (HTTPUACleaner.allBlock !== false)
				{
					if (HTTPUACleaner.allBlock < 3)
					{
						var protocol = HTTPUACleaner.getProtocolFromURL(httpChannel.URI.spec);

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
							return;
						}

						var isAllowedOnStartF = function(host)
						{
							if (HTTPUACleaner.hostsAllowedOnStart[httpChannel.URI.host])
								return true;

							for (var a in HTTPUACleaner.hostsAllowedOnStart)
							{
								if (a.startsWith('.') && host.endsWith(a))
								{
									return true;
								}
							}

							return false;
						};
						var isAllowedOnStart = isAllowedOnStartF(httpChannel.URI.host);
						
						if (HTTPUACleaner.ocspAllowedOnStart || isAllowedOnStart)
						if (topic == 'http-on-opening-request' || topic == 'http-on-modify-request')
						{
							if (HTTPUACleaner.isOCSPRequest(httpChannel))
							{
								console.error('HUAC: OCSP allowed on start: ' + httpChannel.URI.spec);
								httpChannel.setRequestHeader("Cookie", 	undefined, false);
								return;
							}

							if (isAllowedOnStart)
							{
								console.error('HUAC: request allowed on start (request): ' + httpChannel.URI.spec);
								return;
							}
						}
						else
						{
							if (HTTPUACleaner.isOCSPResponse(httpChannel, {}))
							{
								console.error('HUAC: OCSP allowed on start: ' + httpChannel.URI.spec);
								httpChannel.setResponseHeader("Set-Cookie", 	undefined, false);
								return;
							}

							if (isAllowedOnStart)
							{
								console.error('HUAC: request allowed on start (response): ' + httpChannel.URI.spec);
								return;
							}
						}

						// httpChannel.cancel(Cr.NS_ERROR_NOT_INITIALIZED);
						try
						{
							httpChannel.suspend();
							HTTPUACleaner.observer.suspended.push([subjectA, httpChannel]);

							if (HTTPUACleaner.loggerB && HTTPUACleaner.allBlock >= 0)
								HTTPUACleaner.loggerB.addToLog('', false, path, {type: 'suspended beacuse HUAC not initialized', msg: {topic: topic}, level: 8});
						}
						catch (e)
						{
							if (e.result == Cr.NS_ERROR_NOT_AVAILABLE)
							{
								if (HTTPUACleaner.loggerB && HTTPUACleaner.allBlock >= 0)
									HTTPUACleaner.loggerB.addToLog('', false, path, {type: 'block beacuse HUAC not initialized', msg: {topic: topic}, level: 3});
								else
									console.error('block beacuse HUAC not initialized ' + path);
							}
							else
							{
								HTTPUACleaner.logObject(e, true);

								if (HTTPUACleaner.loggerB && HTTPUACleaner.allBlock >= 0)
									HTTPUACleaner.loggerB.addToLog('', false, path, {type: 'block beacuse HUAC not initialized (unknown error)', msg: {topic: topic}, level: 3});
								else
									console.error('block beacuse HUAC not initialized ' + path);
							}

							httpChannel.cancel(Cr.NS_ERROR_NOT_INITIALIZED);
						}

						return;
					}
					else
					{
						HTTPUACleaner.allBlock = false;

						try
						{
							// Вызываем здесь, т.к. именно здесь он точно уже инициализирован
							HTTPUACleaner.sdb.startUpContinue();	// Продолжаем инициализацию, зависимую ранее от storage (шифры)						
						}
						catch (e)
						{
							HTTPUACleaner.logMessage('HUAC FATAL ERROR: startUpContinue error', true);
							HTTPUACleaner.logObject(e, true);
						}

						for (var request of HTTPUACleaner.observer.suspended)
						try
						{
							try
							{
								HTTPUACleaner.observer.observe(request[0]);
								request[1].resume();
							}
							catch (e)
							{
								console.error(e);
								request[1].cancel(Cr.NS_ERROR_NOT_INITIALIZED);
							}
							HTTPUACleaner.loggerB.addToLog('', false, request[1].URI.spec, {type: 'resumed beacuse HUAC has been initialized', level: 8});
						}
						catch (e)
						{
							HTTPUACleaner.logObject(e, true);
						}

						HTTPUACleaner.setPluginButtonState();
					}
				}
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
			}
			
			
			if (HTTPUACleaner.NeedToRestoreAllCiphersState === true)
			{
				HTTPUACleaner.RestoreAllCiphersState();
			}

			// Если это вначале - то срабатывает HTTPUACleaner.allBlock < 3 и сюда не доходит
			// Значит, мы здесь убиваем обработку только когда дополнение уже заканчивает выполнение
			if (HTTPUACleaner.terminated || !subjectA)
			{
				// httpChannel.cancel(Cr.NS_ERROR_NOT_INITIALIZED);
				return;
			}

			try
			{
				if (topic == 'http-on-opening-request')
				{
					HTTPUACleaner.onHttpRequestOpened(subject, false);
					HTTPUACleaner.httpRequestObserved.push(subject);
				}
				else
				if (topic == 'http-on-modify-request')
				{
					//HTTPUACleaner.onHttpRequestModify(subject, topic, data);
					//var httpChannel = subject.QueryInterface(Ci.nsIHttpChannel);
					//console.error("new " + httpChannel.URI.host + httpChannel.URI.path);
					if (HTTPUACleaner.httpRequestObserved.indexOf(subject) == -1)
					{
						HTTPUACleaner.onHttpRequestOpened(subject, false);
						HTTPUACleaner.httpRequestObserved.push(subject);
					}
					else
						HTTPUACleaner.onHttpRequestOpened(subject, true);

					if (!HTTPUACleaner.onHttpResponseReceived.hasBeenRequests && Date.now() - HTTPUACleaner.startupTime > 15000)
						HTTPUACleaner.onHttpResponseReceived.hasBeenRequests = true;
				}
				else
				if (topic == 'http-on-examine-response')
				{/*var si = subject.QueryInterface(Ci.nsIHttpChannel).securityInfo;//.QueryInterface(Ci.nsIHttpChannel);
				if (si instanceof Ci.nsISSLStatusProvider)
					console.error(si.QueryInterface(Ci.nsISSLStatusProvider).SSLStatus.cipherName);*/

					// Для запросов, которые пропущены при запуске, чтобы они в deleteRequestFromArray обрабатывались для http log
					if (!HTTPUACleaner.onHttpResponseReceived.hasBeenRequests && HTTPUACleaner.httpRequestObserved.indexOf(subject) == -1)
					{
						HTTPUACleaner.httpRequestObserved.push(subject);
					}

					HTTPUACleaner.onHttpResponseReceived(subject, topic, data);
					// HTTPUACleaner.deleteRequestFromArray();
				}
				else
				if (topic == 'http-on-examine-cached-response')
				{
					HTTPUACleaner.onHttpResponseCached(subject, topic, data);
					// HTTPUACleaner.deleteRequestFromArray();
				}
				else
				if (topic == 'http-on-examine-merged-response')
				{
					//HTTPUACleaner.onHttpResponseReceived(subject, topic, data);
					HTTPUACleaner.onHttpResponseCached(subject, topic, data);
					// HTTPUACleaner.deleteRequestFromArray();
				}
			}
			catch (e)
			{
				console.error("HUAC: observer error " + path + ' / ' + e);
				console.error({subject: subject, topic: topic, data: data, exception: e});
			}
		},
		
/*
		QueryInterface: XPCOMUtils.generateQI([Ci.nsIObserver])*/
		//QueryInterface: XPCOMUtils.generateQI([Ci.nsIContentPolicy, Ci.nsIFactory]),
		//_classID: components.ID("6d6f6674-2043-4951-aef2-5e909370a4f6"),

		QueryInterface: XPCOMUtils.generateQI([Ci.nsIContentPolicy, Ci.nsIFactory]),
		//QueryInterface: XPCOMUtils.generateQI([Ci.nsISimpleContentPolicy, Ci.nsIFactory]),
		
		// 6EEB771577827E391715FEBF8577229EC5D3E78454856EBBC0404BE5D89344E1C95BA2A1B3AFF57CFF7AF9F233DAE72F49405CBFB5095E7B5DB9BC456E5C288A
		//QueryInterface: HTTPUACleaner.isE10S ? XPCOMUtils.generateQI([Ci.nsISimpleContentPolicy, Ci.nsIFactory]) : XPCOMUtils.generateQI([Ci.nsIContentPolicy, Ci.nsIFactory]),
		_classDescription: "HTTP UserAgentCleaner Content Policy",
		_classID: components.ID("84389FE4-AD9F-7F6D-817A-95B7EC1AD019"),
		_contractID: "@fxprivacy.8vs.ru/policy;1",

		// https://developer.mozilla.org/en-US/docs/Mozilla/Tech/XPCOM/Reference/Interface/nsIContentPolicy
		// http://hg.mozilla.org/mozilla-central/file/9ba87714f5cf/dom/base/nsISimpleContentPolicy.idl
		// https://hg.mozilla.org/mozilla-central/rev/9ba87714f5cf
		shouldLoad: function(contentType, contentLocation, requestOrigin)
		{
			try
			{
				if (!HTTPUACleaner.enabled)
					return Ci.nsIContentPolicy.ACCEPT;
			}
			catch (e)
			{
				return Ci.nsIContentPolicy.ACCEPT;
			}

			// Для feed:http://fxprivacy.8vs.ru/rss.php requestOrigin будет null
			var protocol = HTTPUACleaner.getProtocolFromURL(contentLocation.spec, true); //contentLocation.scheme;
			var po 		 = requestOrigin ? HTTPUACleaner.getProtocolFromURL(requestOrigin.spec, true) : 'moz-nullprincipal';		// Для новой страницы moz-nullprincipal

			if (  HTTPUACleaner.isNoCPProtocol(protocol)  )
				if (po.indexOf('ws') < 0 && po.indexOf('http') < 0 && po.indexOf('javascript') < 0)
					return Ci.nsIContentPolicy.ACCEPT;

			try
			{
				// На всякий случай даём возможность разрешить сертификат как можно раньше
				// Потом дополнительно будем обрабатывать при формировании httpRequest
				var result = this.shouldLoadFunc.apply(this, arguments);
				if (result == Ci.nsIContentPolicy.ACCEPT && (protocol == 'https' || protocol == 'wss'))
				{
					var host = HTTPUACleaner.getHostByURI(contentLocation.spec);
					HTTPUACleaner.certsObject.rootCertificateEnable
					(
						HTTPUACleaner.getDomainByHost(host), host, 1, ''
					);
				}

				return result;
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true);
				return Ci.nsIContentPolicy.REJECT_REQUEST;
			}
		},
		
		shouldLoadFunc:
			function(contentType, contentLocation, requestOrigin, node/*aTopFrameElement*/, aIsTopLevel, mimeTypeGuess, extra, aRequestPrincipal)
			{
				/*
				if (contentLocation.spec.indexOf('chrome') != 0)
				if (node)
				{
					console.error('shouldLoad');
					console.error(contentLocation.spec);
					console.error(node);
					console.error(aRequestPrincipal);
				}*/

				//if (contentLocation.spec.indexOf('') >= 0)
				/*try
				{
				
					
					console.error("shouldLoad");
					console.error("contentType: ");		console.error(contentType);
					console.error("contentLocation: "); console.error(contentLocation.spec);
					console.error("requestOrigin:"); 	console.error(requestOrigin);
					console.error("node:");				console.error(node);
					console.error("mimeTypeGuess:"); 	console.error(mimeTypeGuess);
					console.error("extra:"); 			console.error(extra);
					
					console.error("contentLocation: "); console.error(contentLocation.spec);
					console.error("contentType: ");		console.error(contentType);
					console.error("requestOrigin:"); 	console.error(requestOrigin);
				}
				catch (e)
				{}
*/
				// contentLocation.spec
				// requestOrigin.spec	-	для документов [contentType == 6] - это chrome://browser/content/browser.xul
				// node - элемент, для которого вызывается shouldLoad
				// mimeTypeGuess - ожидаемый тип, может быть пустой строкой
				// extra == null

				// TYPE_OTHER		1
				// TYPE_SCRIPT		2
				// TYPE_IMAGE		3
				// TYPE_STYLESHEET	4
				// TYPE_OBJECT		5
				// TYPE_DOCUMENT	6
				// TYPE_SUBDOCUMENT	7
				// TYPE_PING		8
				// TYPE_XBL			9
				// TYPE_XMLHTTPREQUEST	11
				// TYPE_OBJECT_SUBREQUEST	12		// plugin
				// TYPE_FONT		14
				// TYPE_MEDIA		15
				// TYPE_WEBSOCKET	16
				// REJECT_REQUEST	-1

				var protocol = HTTPUACleaner.getProtocolFromURL(contentLocation.spec, true);
				var notTitle = HTTPUACleaner.notTitle;
				if (HTTPUACleaner.terminated || !HTTPUACleaner.httpOptionsInitialized())
				{
					console.error('HUAC may be allowed or disallowed request because initialization or termination in progress: ' + contentLocation.spec + ' (type: ' + contentType + ' ' + notTitle['' + contentType] + ')');
					return Ci.nsIContentPolicy.ACCEPT;
				}

				var urls = HTTPUACleaner.urls;

				var host = HTTPUACleaner.getHostByURI(contentLocation.spec);
				var requestHost = HTTPUACleaner.getHostByURI(requestOrigin ? requestOrigin.spec : 'moz-nullprincipal:{}');
				if (requestHost != 'browser' && HTTPUACleaner.getProtocolFromURL(requestOrigin ? requestOrigin.spec : 'moz-nullprincipal:{}', true) != 'moz-nullprincipal')
				{
					host = requestHost;
					// console.error("host: " + host);
				}

				var noTab = true;
				var document = undefined;
				var tab = undefined;
				var taburi = undefined;

				try
				{
					if (node.defaultView)
						document = node;
					else
					{
						if (node.ownerDocument)
							document = node.ownerDocument;
						else
							document = node.document;
					}

					taburi 	= null;
					var I 	= urls.utils;
					tab 	= I.getTabForContentWindow(document.defaultView);

					var topLevel = false;
					try					
					{
						if (!tab)
						{
							tab      = I.getTabForContentWindow(node.contentWindow);
							topLevel = true;
						}
					}
					catch (e)
					{
					}

					if (tab)
					{
						taburi  	= urls.fromTabDocument(tab, /*contentType == Ci.nsIContentPolicy.TYPE_DOCUMENT*/ false);
					}

					// Это относится не только к content-policy
					// Это для того, чтобы отслеживать загрузку новых документов во вкладки
					if (contentType == Ci.nsIContentPolicy.TYPE_DOCUMENT)
					{
						var obj     = {};
						obj.url     = contentLocation.spec;
						obj.taburl  = taburi;
						obj.ltu		= taburi;
						obj.time	= Date.now();
						obj.work    = 0;
						HTTPUACleaner.tabsListener.lastUrls.push(obj);

						// В яндексе проходит странный запрос, mail.yandex.ru/#inbox уже когда вкладка загружена. Однако в httpFox он не виден
						if (tab && tab.linkedBrowser)
						{
							if (!tab.linkedBrowser.huac)
								tab.linkedBrowser.huac = {cpurl: '' + contentLocation.spec};
							else
								tab.linkedBrowser.huac.cpurl = '' + contentLocation.spec;

							if (protocol == 'file')
							{
								tab.linkedBrowser.huac.url = contentLocation.spec;
							}
							
							//console.error('content-policy huac.cpurl: ' + contentLocation.spec);
						}

						// utils.getBrowserForTab(tab).huac = {url: contentLocation.spec};

						HTTPUACleaner.lastDocument = {taburl: taburi, url: contentLocation.spec, time: obj.time};

/*
						console.error('TYPE_DOCUMENT ' + contentLocation.spec);
						console.error('origin ' + requestOrigin.spec);
						console.error(HTTPUACleaner.tabsListener.lastUrls);*/
						// chrome://browser/content/browser.xul"
						// moz-nullprincipal:{guid}

						// Изменяем taburi так, чтобы пропускало документ в правилах side
						taburi = contentLocation.spec;
					}

					if (!tab || tab === true)
					{
						/*if (HTTPUACleaner.debug && (!node.baseURI || node.baseURI != 'chrome://browser/content/browser.xul'))
						{
							console.error("HUAC: no tab for document (content-policy)");
						}*/
					}
					else
					{
						if (topLevel)
						{
							// host = HTTPUACleaner.getHostByURI(taburi); // это уже и так выше сделано
							noTab = true;

							// Если новая страница в новой вкладке, сохранить её на случай отмены загрузки изображения или документа
							// HTTPUACleaner.lastHost будет отображаться на пустой вкладке с отменённой загрузкой
							HTTPUACleaner.lastHost = HTTPUACleaner.getHostByURI(contentLocation.spec);
							HTTPUACleaner.lastUri  = contentLocation.spec;
						}
						else
						{
							host = HTTPUACleaner.getHostByURI(taburi);
							noTab = false;
						}
						// console.error("host for " + contentLocation.spec + ": " + host);

					}
				}
				catch (e)
				{/*
					if (HTTPUACleaner.debug)
					{
						console.error("HUAC: tab for content-policy");
						console.error(contentLocation.spec);
						console.error(host);
						HTTPUACleaner.logObject(e, true);
					}*/
				}

				if (noTab)
				{/*
						var protocol = requestOrigin.scheme;
						if (  HTTPUACleaner.isNoCPProtocol(protocol)  )
						// Origin может быть Chrome, если загружается новый документ
						//if (contentType != Ci.nsIContentPolicy.TYPE_DOCUMENT)
						{
							// console.error('allow protocol (without tab) ' + protocol);
							return Ci.nsIContentPolicy.ACCEPT;
						}*/

						// Исполняется при about:blank
						// Признаться, я не очень понял: сюда иногда явно доходят запросы с вкладок
						// Сюда доходят запросы типа data: и т.п.
						if (  HTTPUACleaner.isNoCPProtocol(protocol, true)  )
						{
							return Ci.nsIContentPolicy.ACCEPT;
						}
				}
				else
				{
					var protocol = HTTPUACleaner.getProtocolFromURL(taburi, true);
					if (!taburi || taburi.indexOf('about:home') == 0 || taburi.indexOf('about:newtab') == 0 || taburi.indexOf('about:blank') == 0 || taburi.indexOf('about:neterror') == 0)
					{
						protocol = HTTPUACleaner.getProtocolFromURL(contentLocation.spec, true);
					}

					if (  HTTPUACleaner.isNoCPProtocol(protocol)  )
					{
						// console.error('allow protocol ' + HTTPUACleaner.getProtocolFromURL(taburi, true));
						return Ci.nsIContentPolicy.ACCEPT;
					}
				}

				// Это может помешать правильному логированию на вкладку ''
				if (taburi)
				if (taburi == 'about:home' || taburi == 'about:newtab' || taburi == 'about:blank' || taburi.indexOf('about:neterror') >= 0)
				{
					taburi = contentLocation.spec;
					if (noTab)
						console.error('HUAC error: noTab in tab found (content-policy)');
				}

				if (HTTPUACleaner.getFunctionState(host, "NoFilters") == 'no filters')
					return Ci.nsIContentPolicy.ACCEPT;

				// Блокируем resource:
				var po = HTTPUACleaner.getProtocolFromURL(requestOrigin ? requestOrigin.spec : 'moz-nullprincipal:{}', true);
				var ps = HTTPUACleaner.getProtocolFromURL(contentLocation.spec, true);
				try
				{	// https://www.browserleaks.com/firefox
					if (HTTPUACleaner.resourceDisallow || HTTPUACleaner.resourceDisallowStrong)
					if (po.indexOf('ws') == 0 || po.indexOf('http') == 0 || po.indexOf('javascript') == 0)
					{
						if (ps.indexOf('moz') == 0 || ps.indexOf('resource') == 0 || ps.indexOf('chrome') == 0 || ps.indexOf('view-source') == 0)
						{
							if (
									contentLocation.spec.indexOf('resource://gre') == 0
/*								||  contentLocation.spec.indexOf('resource:///chrome/') == 0
								||  contentLocation.spec.indexOf('resource:///defaults') == 0
								||  contentLocation.spec.indexOf('resource:///components') == 0
								||  contentLocation.spec.indexOf('resource:///jsloader') == 0
								||  contentLocation.spec.indexOf('resource:///jssubloader') == 0
								||  contentLocation.spec.indexOf('resource:///modules') == 0*/
								||  contentLocation.spec.indexOf('resource:///') == 0
							)
							{
								HTTPUACleaner.loggerB.addToLog(taburi, false, contentLocation.spec, {type: 'content policy', level: 3, msg: {'service uri': contentLocation.spec, 'origin uri': (requestOrigin ? requestOrigin.spec : null)}});

								return Ci.nsIContentPolicy.REJECT_REQUEST;
							}
							else
								if (HTTPUACleaner.resourceDisallowStrong)
									return Ci.nsIContentPolicy.REJECT_REQUEST;
						}
					}
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true);
				}

				if (  HTTPUACleaner.isNoCPProtocol(ps, true)  )
					return Ci.nsIContentPolicy.ACCEPT;

				var isOnlyHttpsHost = HTTPUACleaner.isOnlyHttps(host, HTTPUACleaner.getHostByURI(contentLocation.spec));
				var replacedLocation = false;
				if (isOnlyHttpsHost)
				{
					// Проверка, что с версией FireFox всё впорядке
					if (!Ci.nsIContentPolicy.TYPE_SUBDOCUMENT || !Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME || !Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME)
					{
						console.error('!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!');
						console.error('HUAC nsIContentPolicy: have not Ci.nsIContentPolicy.TYPE_SUBDOCUMENT or TYPE_INTERNAL_FRAME or TYPE_INTERNAL_IFRAME');
						console.error('HUAC FATAL ERROR');
						console.error('!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!');
						/*
							console.error('1-: ' + Ci.nsIContentPolicy.TYPE_SUBDOCUMENT);
							console.error('2-: ' + Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME);
							console.error('3-: ' + Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME);*/
					}

					if (
							contentType == Ci.nsIContentPolicy.TYPE_DOCUMENT
							||
							contentType == Ci.nsIContentPolicy.TYPE_SUBDOCUMENT
							||
							contentType == Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME
							||
							contentType == Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME
							||
							contentType == Ci.nsIContentPolicy.TYPE_OTHER
							||
							contentType == Ci.nsIContentPolicy.TYPE_OBJECT
						)
					{
						if (contentType == Ci.nsIContentPolicy.TYPE_DOCUMENT || contentType == Ci.nsIContentPolicy.TYPE_SUBDOCUMENT || contentType == Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME || contentType == Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME || noTab)
						{
							if (contentType == Ci.nsIContentPolicy.TYPE_DOCUMENT)
								HTTPUACleaner.lastHost = host;

/*
							var notifications = require("sdk/notifications");
							notifications.notify
							({
								title: 		HTTPUACleaner['sdk/l10n'].get("Blocked non https") + " " + HTTPUACleaner['sdk/l10n'].get(notTitle["" + contentType]),
								text: 		HTTPUACleaner.notificationNumber + ": " + host  + "\r\n" + contentLocation.spec,
								iconURL: 	HTTPUACleaner['sdk/self'].data.url("HTTPUACleaner.png"),
								data: 		"" + HTTPUACleaner.notificationNumber,
								onClick: 	function() {}
							});*/

							if (!replacedLocation && contentType != Ci.nsIContentPolicy.TYPE_DOCUMENT && !HTTPUACleaner.isNonBlockHttpsFilter())
							try
							{
								var nbox = HTTPUACleaner['sdk/window/utils'].getMostRecentBrowserWindow().gBrowser.getNotificationBox();
								try
								{
									if (!tab)
										nbox = HTTPUACleaner['sdk/tabs/utils'].getTabBrowserForTab(node).getNotificationBox();
									else
										nbox = HTTPUACleaner['sdk/tabs/utils'].getTabBrowserForTab(tab).getNotificationBox();
								}
								catch (e)
								{
									// console.error("FAILED!!!!");
								}

								nbox.appendNotification
								(
									HTTPUACleaner['sdk/l10n'].get("Blocked non https") + " " + HTTPUACleaner['sdk/l10n'].get(notTitle["" + contentType]) +" for " + host  + " / " + contentLocation.spec,
									"HTTPUACleaner",
									HTTPUACleaner['sdk/self'].data.url("HTTPUACleaner.png"),
									"PRIORITY_INFO_HIGH",
									null
								);
							}
							catch(e)
							{
								console.error("HTTPUACleaner notification fail");
								HTTPUACleaner.logObject(e, true);
							}
						}
					}

					if (!replacedLocation && contentType != Ci.nsIContentPolicy.TYPE_DOCUMENT && !HTTPUACleaner.isNonBlockHttpsFilter())
					{
						HTTPUACleaner.loggerB.addToLog(taburi, false, contentLocation.spec, {type: 'Only HTTPS (c-p)', level: 2, msg: {'source': 'content policy', 'origin': (requestOrigin ? requestOrigin.spec : null), 'action': 'disallowed'/*, 'isOnlyHttpsHost': isOnlyHttpsHost*/}});

						return Ci.nsIContentPolicy.REJECT_REQUEST;
					}
					else
					// Это для того, чтобы можно было перенаправить целый документ в обработчике http-запроса
					if (!replacedLocation && (contentType == Ci.nsIContentPolicy.TYPE_DOCUMENT || HTTPUACleaner.isNonBlockHttpsFilter()))
					{
						HTTPUACleaner.loggerB.addToLog(taburi, false, contentLocation.spec, {type: 'Only HTTPS (c-p)', level: 1, msg: {'source': 'content policy', 'origin': (requestOrigin ? requestOrigin.spec : null), 'action': 'skipped'}});
					}
					else
					{
						HTTPUACleaner.loggerB.addToLog(taburi, false, contentLocation.spec, {type: 'Only HTTPS (c-p)', level: 2, msg: {'source': 'content policy', 'origin': (requestOrigin ? requestOrigin.spec : null), 'action': 'disallowed (unknown)'/*, 'isOnlyHttpsHost': isOnlyHttpsHost*/}});

						return Ci.nsIContentPolicy.REJECT_REQUEST;
					}
				}

				// HTTPUACleaner.loggerSide.addToLog(taburi, false, contentLocation.spec, host, {type: 'content-policy', 'content-type': contentType});

				
				var rejectRequest = function(type)
				{
					HTTPUACleaner.loggerB.addToLog(taburi, false, contentLocation.spec, {type: 'content policy', level: type, msg: {'firefox-content-type': notTitle['' + contentType] + ' (' + contentType + ')'}});
					
					return Ci.nsIContentPolicy.REJECT_REQUEST;
				};
				
				
				var loggerSideFunc = function()
				{
					// Работа правил вкладки Side
					if (HTTPUACleaner.loggerSide.enabled)
					try
					{
						TLInfo = {f: true};

						TLInfo.isOCSP       = null;
						TLInfo.minTLSStrong = HTTPUACleaner.minTLSStrong;
						TLInfo.origin = null;
						
						var isPrivate = function(tab)
						{
							if (tab === true || !tab)
							{
								TLInfo.haveContext = false;
								TLInfo.isPrivate   = false;
								// console.error('no tab for ' + contentLocation.spec);
								return false;
							}

							//var bw = urls.utils.getBrowserForTab(tab);
							TLInfo.isPrivate   = HTTPUACleaner['sdk/private-browsing'].isPrivate(urls.chromeTabToSdkTab(tab));
							TLInfo.haveContext = true;
/*
							console.error(contentLocation.spec);
							console.error('isPrivate ' + TLInfo.isPrivate);
							*/
						};
						
						try
						{
							isPrivate(tab);
						}
						catch (e)
						{
							TLInfo.isPrivate   = false;
							TLInfo.haveContext = false;
							HTTPUACleaner.logObject(e, true);
						}

						// obj.taburl obj.redirectTo
						obj = {};
						obj.turl = taburi ? taburi : contentLocation.spec;
						var f = HTTPUACleaner.sdb.checkRules.response.bind(HTTPUACleaner.sdb)(contentLocation.spec, obj, {rtype: undefined, ftype: notTitle['' + contentType].split(' ')[0].split(':')[0]}, TLInfo, 'content-policy');
/*
if (contentLocation.spec.indexOf('anonymox.net') >= 0)
{
	console.error('noTab: ' + noTab);
	console.error(contentLocation.spec);
	console.error(taburi);
	console.error(f);
}*/

						if (f.CertsHPKP == 2)
							try
							{
								if ((contentLocation.scheme == 'https' || contentLocation.scheme == 'wss') && HTTPUACleaner.HPKP_ResetArray && HTTPUACleaner.HPKP_ResetArray[contentLocation.host])
								{
									HTTPUACleaner.certsObject.clearHostHPKP(contentLocation.host);
									delete HTTPUACleaner.HPKP_ResetArray[contentLocation.host];
								}
							}
							catch (e)
							{
								console.error('HUAC error in CertsHPKP:X');
								HTTPUACleaner.logObject(e, true);
								try
								{
									console.error(contentLocation.spec);
								}
								catch (ex)
								{}
							}

						if (f.executed === true || f.log.level != 1)
						{
							f.log.msg.ft = notTitle['' + contentType].split(' ')[0].split(':')[0].toUpperCase();
							HTTPUACleaner.loggerB.addToLog(taburi, false, contentLocation.spec, f.log);
						}

						if (f.cancel === true)
						{
							if (
								(contentType == Ci.nsIContentPolicy.TYPE_IMAGE || contentType == Ci.nsIContentPolicy.TYPE_IMAGESET)
								&& HTTPUACleaner.searchPassImg(contentLocation.spec)
								)
							{
								// отказываемся от блокирования изображения, подгружаемого по клику
								f.log.msg['image allowed'] = 'allowed by load image by click';
								f.cancel === false;
							}
							else
							{
								if (contentType == Ci.nsIContentPolicy.TYPE_DOCUMENT)
								{
									var sdkTab = urls.chromeTabToSdkTab(tab);
									if (sdkTab)
									sdkTab.url = HTTPUACleaner['sdk/self'].data.url('blocked.html') + '?' + contentLocation.spec;
								}

								return Ci.nsIContentPolicy.REJECT_REQUEST;
							}
						}
							
						// Правила на куки допустимы - они могут сработать всегда, однако здесь мы на них не реагируем
					}
					catch (e)
					{
						console.error("FATAL ERROR");
						console.error("HTTPUACleaner.sdb.checkRules.response in content-policy has raised exception");
						console.error(taburi);
						console.error(contentLocation.spec);
						HTTPUACleaner.logObject(e, true);
					}
					
					return false;
				};

				if (HTTPUACleaner.loggerSide.enabled)
				{
					var t = loggerSideFunc();
					if (t !== false)
						return t;
				}

				switch (contentType)
				{
					case Ci.nsIContentPolicy.TYPE_IMAGE:
					case Ci.nsIContentPolicy.TYPE_IMAGESET:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "Images");
						if (state != 'disabled' && !HTTPUACleaner.searchPassImg(contentLocation.spec))
						{
							return rejectRequest(7);
						}
						
						break;
					case Ci.nsIContentPolicy.TYPE_SCRIPT:
					case Ci.nsIContentPolicy.TYPE_XSLT:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "OnlyHtml");
						if (state == 'html' || state == 'css/xml')
						{
							return rejectRequest(6);
						}
						break;
					case Ci.nsIContentPolicy.TYPE_STYLESHEET:
					case Ci.nsIContentPolicy.TYPE_XBL:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "OnlyHtml");
						if (state == 'html')
						{
							return rejectRequest(6);
						}
						break;
					case Ci.nsIContentPolicy.TYPE_DTD:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "OnlyHtml");
						if (state == 'html')
						{
							return rejectRequest(6);
						}
						break;
					case Ci.nsIContentPolicy.TYPE_FONT:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "OnlyHtml");
						if (state != 'disabled')
						{
							return rejectRequest(6);
						}
						break;
					case Ci.nsIContentPolicy.TYPE_OTHER:
					case Ci.nsIContentPolicy.TYPE_OBJECT:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "OnlyHtml");
						if (state != 'disabled')
						{
							return rejectRequest(6);
						}
						break;
					// Запрос плагином видео может быть типа TYPE_OBJECT_SUBREQUEST, а не TYPE_MEDIA
					case Ci.nsIContentPolicy.TYPE_OBJECT_SUBREQUEST:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var stateh = HTTPUACleaner.getFunctionState(host, "OnlyHtml");
						var statea = HTTPUACleaner.getFunctionState(host, "Audio");
						if (stateh != 'disabled' && statea != 'disabled')
						{
							return rejectRequest(6);
						}
						break;
					case Ci.nsIContentPolicy.TYPE_MEDIA:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "Audio");
						if (state != 'disabled')
						{
							return rejectRequest(7);
						}
						break;
					case Ci.nsIContentPolicy.TYPE_WEBSOCKET:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "WebSocket");
						if (state != 'disabled')
						{
							return rejectRequest(6);
						}
						break;
					case Ci.nsIContentPolicy.TYPE_XMLHTTPREQUEST:
					case Ci.nsIContentPolicy.TYPE_BEACON:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "AJAX");
						if (state != 'disabled')
						{
							return rejectRequest(6);
						}
						break;
					// https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API
					case Ci.nsIContentPolicy.TYPE_FETCH:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state = HTTPUACleaner.getFunctionState(host, "Fetch");
						if (state != 'disabled')
						{
							return rejectRequest(6);
						}
						break;
					case Ci.nsIContentPolicy.TYPE_DOCUMENT:
					case Ci.nsIContentPolicy.TYPE_SUBDOCUMENT:
					case Ci.nsIContentPolicy.TYPE_INTERNAL_FRAME:
					case Ci.nsIContentPolicy.TYPE_INTERNAL_IFRAME:
						break;
					default:
					
						if (HTTPUACleaner.isNoCPProtocol(ps))
							break;

						var state1 = HTTPUACleaner.getFunctionState(host, "AJAX");
						var state2 = HTTPUACleaner.getFunctionState(host, "OnlyHtml");
						
						if (state1 != 'disabled' && state2 != 'disabled')
						{
							return rejectRequest(6);
						}
						break;
				}

				return Ci.nsIContentPolicy.ACCEPT;		// Ci.nsIContentPolicy.REJECT_REQUEST
			},

		// Не работает вообще, что ли? Даже не входит в функцию, кажется
		shouldProcess:
			function()
			{
				return Ci.nsIContentPolicy.ACCEPT;
			},
		createInstance:
			function(outer, iid)
			{
				if (outer)
				  throw Cr.NS_ERROR_NO_AGGREGATION;

				return HTTPUACleaner.observer.QueryInterface(iid);
			}
		};

	HTTPUACleaner.cobserver =     observer;
	HTTPUACleaner.observer  = new observer();
};
