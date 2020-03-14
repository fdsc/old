//   ----------------------------------------------------------------------------------
//   ----------- Ниже идут общие функции и функции вкладок FireFox и HTTP -------------
//   ----------------------------------------------------------------------------------


var HTTPUACleanerScripting = 
{
	getOnClickElement:
		function (element, HTTPUACleaner, options)
		{
			if (options.name == "MUA" && options.special == "hold")
				return function() {};

			var currentDomain = "";
			if (options.special == "Domain")
				currentDomain = HTTPUACleanerScripting.currentDomain;
			else
			if (options.special == "Host")
				currentDomain = HTTPUACleanerScripting.currentHost;

			var emitFunc = HTTPUACleanerScripting.getClickOnOptionsEvent({name: options.name, special: options.special, isCurrentDomainKnown: options.isCurrentDomainKnown, currentDomain: currentDomain});
			return function()
				{

					if (!options.isCurrentDomainKnown && (options.special == "Domain" || options.special == "Host"))
						return;

					var nextState = HTTPUACleanerScripting.getNextMainOptionsValue
										(
											element.HTTPUACleanerState, options.special, options.name,
											(options.special == "Domain" || options.special == "Host")
										);

					try
					{
						HTTPUACleanerScripting.showColoredElement(nextState, element, HTTPUACleaner);
					}
					catch (e)
					{
						console.error("getOnClickElement error " + e);
						console.error(e);
					}

					emitFunc(nextState);

				};
		},
	getOnClickPriorityElement:
		function (element, HTTPUACleaner, options)
		{
			var currentDomain = "";
			if (options.special == "Domain")
				currentDomain = HTTPUACleanerScripting.currentDomain;
			else
			if (options.special == "Host")
				currentDomain = HTTPUACleanerScripting.currentHost;

			var emitFunc = HTTPUACleanerScripting.getClickOnPriorityOptionsEvent({name: options.name, special: options.special, isCurrentDomainKnown: options.isCurrentDomainKnown, currentDomain: currentDomain});
			return function()
				{
					if (!options.isCurrentDomainKnown && (options.special == "Domain" || options.special == "Host"))
						return;

					var nextState = element.HTTPUACleanerState > 4 ? 1 : element.HTTPUACleanerState + 1;
					HTTPUACleanerScripting.showColoredPriorityElement(nextState, element, HTTPUACleaner);

					emitFunc(nextState);
				};
		},

	// Дублируется в lib/main.js
	mainOptionsValuesEnum: ['none', "enabled", "disabled", "random", "low random", "raise error", "no cache", "no persistent", 'validate', 'cache', "html", "css/xml", "js", "host", "clean", "Firefox 28", "Opera 12.14", "Chrome 33", "IE 10.0", "Googlebot", "track", "no track", "1", "2", "3", "4", "5", "6", "7", "8", "11", "no filters", 'click', 'domain', 'isolated', 'font', 'en-us'],

	allowed:
	{
		'Fonts':   	['enabled',				  'random',		'disabled'/*, 'raise error'*/],
		'Plugins': 	['enabled', 			  				'disabled', 'raise error'],
		'UA':		['enabled', 'low random', 'random', 	'disabled', 'raise error'],
		'Referer':	['enabled', 'host',		  'domain',	  	'disabled'],
		'XForwardedFor':       
					['enabled', 		      'random', 	'disabled'],
		'Storage':	['enabled',								'disabled', 'raise error'],
		'Caching':	['no cache', 'no persistent',			'disabled', 'validate', 'cache'],
		'TimeZone':	['enabled', 							'disabled', 'raise error'],
		'Etag':		['enabled', 							'disabled'],
		'WebGL':	['enabled',								'disabled'],
		'AudioCtx': ['enabled',								'disabled'],
		'Canvas':	['enabled', 							'disabled', 'raise error', 'clean', 'low random', 'random',  'font'],
		'OnlyHtml': ['html', 	'css/xml', 'js', 			'disabled'],
		'Authorization':
					['enabled', 							'disabled'],
		'hCookies': ['enabled', 							'disabled', 'host', 'domain'/*, 'isolated'*/],
		'dCookies': ['enabled', 							'disabled', 'raise error'],
		'Images':	['enabled', 							'disabled', 'click'],
		'Audio':	['enabled', 							'disabled'],
		'AcceptHeader':
					['enabled', 							'disabled', 'clean'],
		'WebRTC':	['enabled',								'disabled'],
		'PushAPI':	['enabled',								'disabled'],
		'ServiceWorker':
					['enabled',								'disabled'],
		'WebSocket':['enabled', 							'disabled'],
		'Fetch':	['enabled', 							'disabled'],
		'AJAX':		['enabled', 							'disabled'],
		'CORS':		['enabled',								'disabled', 'clean', 'domain'],
		'wname':	['enabled', 							'disabled', 'raise error', 'clean'],
		'MUA':		['disabled', 	'Firefox 28', 'Opera 12.14', 'Chrome 33', 'IE 10.0', 'Googlebot'],
		'UATI':		['disabled', 	'1', '2', '3', '4', '5', '6', '7', '8', '11'],
		'DNT':		['track',		'no track',	'random',	'disabled', 'clean'],
		'NoFilters':['disabled',	'no filters'],
		'Locale':   ['enabled', 							'disabled', 'en-us'],
		'Screen':	['enabled', 							'disabled'],
		'OnlyHttps':['enabled', 							'disabled'],
		'Password': ['enabled',								'disabled']
	},

	isAllowedOptionsValue: function(value, special, name, noneAllowed)
	{
		if (value == "none")
			return noneAllowed;

		return (HTTPUACleanerScripting.allowed[name].indexOf(value) >= 0);
	},

	getNextMainOptionsValue: function(value, special, name, noneAllowed)
	{
		var i = HTTPUACleanerScripting.mainOptionsValuesEnum.indexOf(value);
		
		var counter = HTTPUACleanerScripting.mainOptionsValuesEnum.length;
		do
		{
			if (i < 0 || i >= HTTPUACleanerScripting.mainOptionsValuesEnum.length - 1)
				i = 0;
			else
				i++;
			
			counter--;
			if (counter < 0)
			{
				throw new RangeError("getNextMainOptionsValue is infinite loop cycle with value " + value + " and name " + name);
				break;
			}
		}
		while (!HTTPUACleanerScripting.isAllowedOptionsValue(HTTPUACleanerScripting.mainOptionsValuesEnum[i], special, name, noneAllowed));
		
		return HTTPUACleanerScripting.mainOptionsValuesEnum[i];
	},

	// дублировано в main.js
	getDomainByHost: function(host)
	{
		if (!host)
			return "";

		try
		{
			var regex = /[^.]+\.[^.]+$/i;
			var result = regex.exec(host);
			result = result[0];

			if (!result)
				return "";
			else
				return result;
		}
		catch (exception)
		{
			return host;
		}
	},

	showColoredElement: function(state, e, HTTPUACleaner)
	{
		e.HTTPUACleanerState = state;
		e.textContent = HTTPUACleanerScripting.mainOptionsCaptions[state] || "ERROR";
		if (e.textContent == 'css/xml')
			e.textContent = 'css';
		else
		if (e.textContent == 'js')
			e.textContent = 'js/xml';

		if (HTTPUACleaner.enabled)
		{
			e.style.backgroundColor = HTTPUACleaner.mainOptionsColors[state] || "black";

			if (e.style.backgroundColor == 'rgb(0, 0, 0)' || e.style.backgroundColor == 'rgb(0, 0, 255)')
				e.style.color = 'white';
			else
				e.style.color = 'black';
		}
		else
			e.style.backgroundColor = "#FFFFCC";
	},
	
	showGrayElement: function (state, e)
	{
		e.textContent = "";
		e.style.backgroundColor = "#888888";
	},

	showColoredPriorityElementColors: ["#000000", "#FF0000", "#FF8800", "#FFFF00", "#88FF00", "#00FF00"],
	
	showColoredPriorityElement: function(state, e, HTTPUACleaner)
	{
		state = state || 5;
		e.HTTPUACleanerState = state;
		e.textContent = state;

		if (HTTPUACleaner.enabled)
			e.style.backgroundColor = HTTPUACleanerScripting.showColoredPriorityElementColors[state];
		else
			e.style.backgroundColor = "#FFFFCC";
	},

	currentDomain: "",
	currentHost:   "",
	
	getDisableAllEvent: function(eventArgs, mainOptionsColors)
	{
		var eventName = eventArgs ? "enableCleaner" : "disableCleaner";

		return function()
		{
			self.port.emit
			(
				eventName,
				eventArgs
			);

			var disableAllElement = document.getElementById("DisableAll");
			HTTPUACleanerScripting.setDisableAllColor(disableAllElement, eventArgs, mainOptionsColors);

			disableAllElement.onclick = HTTPUACleanerScripting.getDisableAllEvent(!eventArgs, mainOptionsColors);
		}
	},
	
	getClickOnOptionsEvent: function(eventArgs)
	{
		var eventName = 'clickOnOptions';

		return function(newState)
		{
			eventArgs.newState = newState;
			self.port.emit
			(
				eventName,
				eventArgs
			);
		}
	},
	
	getClickOnPriorityOptionsEvent: function(eventArgs)
	{
		var eventName = 'clickOnPriorityOptions';

		return function(newState)
		{
			eventArgs.newState = newState;
			self.port.emit
			(
				eventName,
				eventArgs
			);
		}
	},
	
	setDisableAllColor: function(disableAllElement, enabled, mainOptionsColors)
	{
		if (enabled)
		{
			disableAllElement.textContent = HTTPUACleanerScripting.mainOptionsCaptions["Do to disable all"] + " (" + HTTPUACleanerScripting.version + ")";
			disableAllElement.style.backgroundColor = mainOptionsColors['disabled'];
		}
		else
		{
			disableAllElement.textContent = HTTPUACleanerScripting.mainOptionsCaptions["Do to enable all"];
			disableAllElement.style.backgroundColor = mainOptionsColors['high enabled'];
		}
	},


	getOnClickCipherElement:
		function (element, cName, state, targetWhich, callback)
		{
			// e, options.HTTPUACleaner, {isCurrentDomainKnown: isCurrentDomainKnown, special: "Hold", name: name}
			element.HTTPUACleaner_cName = cName;

			var getClickOnOptionsEvent = function()
			{
				var eventName = 'clickOnCipherOptions';

				return function(newState, disabled)
				{
					eventArgs = {};
					eventArgs.newState = newState;
					eventArgs.cName    = cName;
					eventArgs.disabled = disabled;
					self.port.emit
					(
						eventName,
						eventArgs
					);
				};
			};

			var eventFunction = new Object();
			eventFunction.el = element;

			var emitFunc = getClickOnOptionsEvent();
			var result = function(eventArgs)
				{
					eventArgs.cancelBubble = true;

					if (eventArgs.which != targetWhich)
						//state = state == "+" ? "-" : "+";
						return false;

					emitFunc(state/*element.cihperstate == "-" ? "+" : "-", HTTPUACleanerScripting.ciphersDisabled*/);

					if (callback)
						callback(eventArgs, eventFunction);
					return false;
				};

			return result;
		},

	ciphersState: {"?": "+/-", "+": "+", "-": "-", "x": "X"},
	ciphersStateColor: {"?": "#CCCCCC", "+": "#00FF00", "-": "#FF0000", "x": "#000000"},
	ciphersDisabled: [],
	showCipherColoredElement: function(cName, e, ciphers, se)
	{
		state = HTTPUACleanerScripting.getCiphersState(cName, ciphers);
		/*if (state == "-")
			HTTPUACleanerScripting.ciphersDisabled.push(cName);
		*/
		e.cihperstate = state;
		e.textContent = HTTPUACleanerScripting.ciphersState[state];
		e.style.backgroundColor = HTTPUACleanerScripting.ciphersStateColor[state];
		if (e.style.backgroundColor == 'rgb(0, 0, 0)')
			e.style.color = "#FFFFFF";
		else
			e.style.color = "#000000";
	},

	showCipherColoredElements: function(ciphers)
	{
		if (ciphers === false)
			return;

		HTTPUACleanerScripting.ciphersDisabled = [];

		var ciphersNames = ["DES3", "RC4", 'chacha20', "AES128", "AES128GCM", "AES", "AES256GCM", "MD5", "SHA1", "SHA256", "SHA384"/*, "DSS"*/, "RSA", "RSA_PFS", "DHE", "ECDHE", "ECDSA", 'poly1305'];

		for (var c in ciphersNames)
		{
			var ce 		 = ciphersNames[c];
			var cc       = ce; //ciphersCodes[c];

			var e 		 = document.getElementById(ce);
			var ee 	     = document.getElementById("e" + ce);
			var se 	     = document.getElementById("s" + ce);

			if (se .listenerclick)
				se.removeEventListener("click", 		se.listenerclick);
			if (se .listenercontextmenu)
				se.removeEventListener("contextmenu", 	se.listenercontextmenu);
			/*if (ee.listenerclick)
				ee.removeEventListener("click", 	 	ee.listenerclick);*/

			HTTPUACleanerScripting.showCipherColoredElement(ce,  e, ciphers);
			HTTPUACleanerScripting.showCipherColoredElement(ce, se, ciphers.options);

			var e1 = HTTPUACleanerScripting.getOnClickCipherElement(se, cc, se.cihperstate == "-" ? "+" : "-", 1);
			var e2 = HTTPUACleanerScripting.getOnClickCipherElement(se, cc, se.cihperstate == "+" ? "-" : "+", 3);
			//var e3 = HTTPUACleanerScripting.getOnClickCipherElement(ee, cc, "+", 1);

			se.listenerclick 		= e1;
			se.listenercontextmenu 	= e2;
			//ee.listenerclick 		= e3;
			se.addEventListener("click",    	e1);
			se.addEventListener("contextmenu", 	e2);
			//ee.addEventListener("click",    	e3);
		}
	},

	getCiphersState: function(cName, ciphers)
	{
		if (ciphers.enabled.length == 0 && ciphers.disabled.length == 0)
			return "-";

		var disabled = true;
		var enabled  = true;
		for (var c in ciphers.enabled)
		{
			//if (ciphers.enabled[c].indexOf(cName) >= 0)
			if (ciphers.enabled[c] == cName)
			{
				disabled = false;
				break;
			}
		}

		for (var c in ciphers.disabled)
		{
			//if (ciphers.disabled[c].indexOf(cName) >= 0)
			if (ciphers.disabled[c] == cName)
			{
				enabled = false;
				break;
			}
		}

		if (disabled && enabled)
			return "x";

		if (!disabled && !enabled)
			return "?";

		return enabled ? "+" : "-";
	},

	getOnClickOptions1Element:
		function (element, cName, nexState)
		{
			var getClickOnOptionsEvent = function(eventArgs)
			{
				var eventName = 'clickOnOptions1Options';

				return function()
				{
					eventArgs = {};
					eventArgs.newState = nexState;
					eventArgs.cName    = cName;
					self.port.emit
					(
						eventName,
						eventArgs
					);
				};
			};

			var emitFunc = getClickOnOptionsEvent();
			return function()
				{
					emitFunc();
				};
		},
	
	showOptions1ColoredElement: function(option, e, optionState)
	{
		var cl = optionState.getColor(option.val);

		e.style.backgroundColor = cl[0];
		e.textContent = cl[1];

		e.onclick     = HTTPUACleanerScripting.getOnClickOptions1Element(e, option.name, cl[2]);
	},

	option1GetColorGR: function(state)
	{
		if (state === false)
			return ["#00FF00", "-", true];

		return ["#FF0000", "+", true];
	},
	
	option1GetColorGRO: function(state)
	{
		if (state === false)
			return ["#00FF00", "-", true];

		return ["#FF8800", "+", false];
	},

	option1GetColorRG: function(state)
	{
		if (state === false)
			return ["#FF0000", "-", true];

		return ["#00FF00", "+", false];
	},
	
	option1GetColorRY: function(state)
	{
		if (state === false)
			return ["#FFFF00", "+", true];

		return ["#00FF00", "-", false];
	},
	
	option1GetColorYR: function(state)
	{
		if (state === false)
			return ["#00FF00", "-", true];

		return ["#FFFF00", "+", false];
	},

	option1GetColorParallelLimit: function(state)
	{
		if (state !== 0)
			return ["#FFFF00", state, 0];

		return ["#00FF00", state, '+'];
	},
	
	option1GetColorYGG: function(state)
	{
		if (state === false)
			return ["#CCDD00", "-", true];

		return ["#00FF00", "+", false];
	},
	
	option1GetColorGGY: function(state)
	{
		if (state === true)
			return ["#CCDD00", "-", false];

		return ["#00FF00", "+", true];
	},

	option1GetColorRGBlock: function(state)
	{
		if (state === false)
			return ["#FF0000", "-", true];

		return ["#00FF00", "+", true];
	},

	option1GetColorRGO: function(state)
	{
		if (state === false)
			return ["#FF8800", "-", true];

		return ["#00FF00", "+", false];
	},
	
	option1GetColorRGRO: function(state)
	{
		if (state === false)
			return ["#FF5500", "-", true];

		return ["#00FF00", "+", false];
	},

	option1GetColorDocumentFonts: function(state)
	{
		if (state == 0)
			return ["#00FF00", "-", 1];
		else
		if (state == 1)
			return ["#88FF00", "+", 0];
		else
		if (state == 2)
			return ["#FF0000", "2", 0];
	},
	
	option1GetNavigatorColor: function (state)
	{
		if (state == 0)
			return ["#CCCCCC", "+/-", -1];
		else
		if (state == 1)
			return ["#00FF00", "+", 0];
		else
		//if (state == -1)
			return ["#FF0000", "-", 1];
	},
	
	option1GetColorPluginsDefaultState: function(state)
	{
		if (state == 0)
			return ["#00FF00", "disabled", 1];

		if (state == 1)
			return ["#88FF00", "demand", 2];

		if (state == 2)
			return ["#FF0000", "enabled", 0];
	},

	option1GetColorTLS: function(state)
	{
		if (state == 0)
			return ["#FF0000", "SSL 3.0", 1];
		else
		if (state == 1)
			return ["#FFFF00", "TLS 1.0", 2];	// #FF5500
		else
		if (state == 2)
			return ["#AAFF00", "TLS 1.1", 3];
		else
		if (state == 3)
			return ["#00FF00", "TLS 1.2", 4];
		else
		if (state == 4)
			return ["#00FF00", "TLS 1.3", 1];
		else
			return ["#CCCCCC", "?", 1];
	},

	
	option1OSCPEnabled: function (state)
	{
		if (state == 0)
			return ["#FF0000", "-", 1];
		else
		if (state == 1)
			return ["#00FF00", "+", 0];
		else
		//if (state == -1)
			return ["#FF5500", "!", 1];
	},

	option1OcspNoCheck: function (state)
	{
		if (!state)
			return ["#00FF00", state, 20];
		else
		if (state >= 20)
			return ["#FF0000", state, 10];
		else
		if (state > 10)
			return ["#FF" +  Math.floor(255*(2 - state/10)).toString(16)  + '00', state, 0];
		else
			return ["#88" +  Math.floor(255*(2 - state/10)).toString(16)  + '00', state, 0];
	},

	Option1Names: function()
				{
					return {
						// https://wiki.mozilla.org/Security:Renegotiation#security.ssl.renego_unrestricted_hosts
						"gfx.downloadable_fonts.enabled": 
							{
								name: "DwnFonts",
								getColor: HTTPUACleanerScripting.option1GetColorGRO
							},
						"gfx.downloadable_fonts.sanitize": 
							{
								name: "SanFonts",
								getColor: HTTPUACleanerScripting.option1GetColorRG
							}, 
						"browser.display.use_document_fonts": 
							{
								name: "DocFonts",
								getColor: HTTPUACleanerScripting.option1GetColorDocumentFonts
							},
						"security.ssl.treat_unsafe_negotiation_as_broken": 
							{
								name: "BrkNeg",
								getColor: HTTPUACleanerScripting.option1GetColorRG
							},
						"security.ssl.require_safe_negotiation": 
							{
								name: "ReqNeg",
								getColor: HTTPUACleanerScripting.option1GetColorRG
							},
						"security.tls.version.min": 
							{
								name: "minTLS",
								getColor: HTTPUACleanerScripting.option1GetColorTLS
							},
						"security.tls.version.max": 
							{
								name: "maxTLS",
								getColor: HTTPUACleanerScripting.option1GetColorTLS
							},
						"plugin.default.state":
							{
								name: "PlugBlock",
								getColor: HTTPUACleanerScripting.option1GetColorPluginsDefaultState
							},
						"security.xpconnect.plugin.unrestricted":
							{
								name: "PlugConnnect",
								getColor: HTTPUACleanerScripting.option1GetColorGRO
							},
						"network.dns.disablePrefetch": 
							{
								name: "DNSPrefetch",
								getColor: HTTPUACleanerScripting.option1GetColorRY
							},
						"network.prefetch-next": 
							{
								name: "LinkPrefetch",
								getColor: HTTPUACleanerScripting.option1GetColorYR
							},
						"network.http.speculative-parallel-limit": 
							{
								name: "LinkPreconnection",
								getColor: HTTPUACleanerScripting.option1GetColorParallelLimit
							},
						"dom.navigator-property.disable.userAgent":
							{
								name: "NavigatorUA",
								getColor: HTTPUACleanerScripting.option1GetNavigatorColor
							},
						"dom.navigator-property.disable.mozId":
							{
								name: "NavigatorMoz",
								getColor: HTTPUACleanerScripting.option1GetNavigatorColor
							},
						"security.OCSP.enabled":
							{
								name: "ocspMakeCheck",
								getColor: HTTPUACleanerScripting.option1OSCPEnabled
							},
						"security.OCSP.require":
							{
								name: "ocspStrong",
								getColor: HTTPUACleanerScripting.option1GetColorRGRO
							},
						"security.pki.cert_short_lifetime_in_days":
							{
								name: "ocspNoCheck",
								getColor: HTTPUACleanerScripting.option1OcspNoCheck
							},
						'webgl.disabled':
							{
								name: "webgl_firefox",
								getColor: HTTPUACleanerScripting.option1GetColorYGG
							},
						'media.peerconnection.enabled':
							{
								name: "webrtc_firefox",
								getColor: HTTPUACleanerScripting.option1GetColorGGY
							}
					};
				},

	showOption1ColoredElements: function(options)
	{
		var optionStates = HTTPUACleanerScripting.Option1Names();

		for (var c in options)
		{
			var option 		 = options[c];
			var e = document.getElementById(optionStates[option.name].name);

			HTTPUACleanerScripting.showOptions1ColoredElement(option, e, optionStates[option.name]);
		}
	},

	showTLSLog: function(TLSTable)
	{
		// For review editor
		// See data/mainjs/getTLSLogTableObject.js, function getTLSLogTableObject (generate and sanitization)
		// Events showTLSLog (in this and in main.js) and setTLSLog (in this and in main.js)
		document.getElementById("entireLog").textContent = '';
		
		if (!TLSTable)
			return;

		HTTPUACleaner_LogBView.setView(document.getElementById("entireLog"), TLSTable);
	},
	
	showBLog: function(BTable)
	{
		// For review editor
		// See data/mainjs/getBLogTableObject.js, function getBLogTableObject (generate and sanitization)
		// Events showBLog (in this and in main.js) and setBLog (in this and in main.js)
		document.getElementById("entireLogB").textContent = '';
		
		if (!BTable)
			return;

		HTTPUACleaner_LogBView.setView(document.getElementById("entireLogB"), BTable);
	},

	showTLSLogCount: 0,
	showBLogCount: 0,
	showSLogCount: 0,
	SideLogEventRegistered: 0,

	getOptions: function(options)
	{
		try
		{
			document.getElementById("entireLog") .textContent = '';
			document.getElementById("entireLogB").textContent = '';
			document.getElementById("entireSide").textContent = '';
		}
		catch (e)
		{
			console.error(e);
			return;
		}

		/*document.getElementById("mainbody").style.color = 'black';
		document.getElementById("mainbody").style.font  = options.panel.font;*/
		for (var el of document.getElementsByTagName('*'))
		{
			el.style.color = 'black';
			el.style.font  = options.panel.font;
			if (el.className.split(' ').indexOf('bold') >= 0)
				el.style.fontWeight = 'bold';
			if (el.className.split(' ').indexOf('w20')>= 0)
				el.style.fontSize = '20px';
		}

		if (document.defaultView.innerHeight < options.panel.height)
		{
			self.port.on
			(
				'heightDownChanged',
				function(options)
				{
					for (var el of document.getElementsByTagName('*'))
					{
						el.style.color = 'black';
						el.style.font  = options.panel.font;
						
						if (el.className.split(' ').indexOf('bold') >= 0)
							el.style.fontWeight = 'bold';
						if (el.className.split(' ').indexOf('w20') >= 0)
							el.style.fontSize = '20px';
					}
				}
			);

			self.port.emit
			(
				'heightDown',
				{
					real: 	document.defaultView.innerHeight,
					setted:	options.panel.height
				}
			);
			
		}

		document.getElementById("FireFox").onmouseenter = function ()
		{
			document.getElementById("FireFoxSettings").style.display = "block";
			document.getElementById("CiphersSettings").style.display = "block";
			document.getElementById("ResetFireFox")   .style.display = "block";
			document.getElementById("CertificatesDiv").style.display = "block";
			document.getElementById("SelfSettings")   .style.display = "none";
			document.getElementById("entireLog")   	  .style.display = "none";
			document.getElementById("entireLogB")  	  .style.display = "none";
			document.getElementById("entireSide")  	  .style.display = "none";
			
			document.getElementById("FireFox").style.backgroundColor = "#FFFF55";
			document.getElementById("General").style.backgroundColor = "#CCCCCC";
			document.getElementById("Log")	  .style.backgroundColor = "#CCCCCC";
			document.getElementById("LogB")	  .style.backgroundColor = "#CCCCCC";
			document.getElementById("Side")	  .style.backgroundColor = "#CCCCCC";
			
			document.getElementById("mainbody").style.backgroundColor = "#AAAAAA";
		};
		
		document.getElementById("General").onmouseenter = function ()
		{
			document.getElementById("FireFoxSettings").style.display = "none";
			document.getElementById("CiphersSettings").style.display = "none";
			document.getElementById("ResetFireFox")   .style.display = "none";
			document.getElementById("CertificatesDiv").style.display = "none";
			document.getElementById("SelfSettings")   .style.display = "block";
			document.getElementById("entireLog")   	  .style.display = "none";
			document.getElementById("entireLogB")  	  .style.display = "none";
			document.getElementById("entireSide")  	  .style.display = "none";
			
			document.getElementById("FireFox").style.backgroundColor = "#CCCCCC";
			document.getElementById("General").style.backgroundColor = "#FFFF55";
			document.getElementById("Log")	  .style.backgroundColor = "#CCCCCC";
			document.getElementById("LogB")	  .style.backgroundColor = "#CCCCCC";
			document.getElementById("Side")	  .style.backgroundColor = "#CCCCCC";
			
			document.getElementById("mainbody").style.backgroundColor = "#AAAAAA";
		};
		
		if (options.TLS.tp == 2)
			document.getElementById("FireFox").onmouseenter();
		else
			document.getElementById("General").onmouseenter();

		var logMouseOverFunc = function(TLSLogUrl)
		{
			return function ()
			{
				document.getElementById("FireFoxSettings").style.display = "none";
				document.getElementById("CiphersSettings").style.display = "none";
				document.getElementById("ResetFireFox")   .style.display = "none";
				document.getElementById("CertificatesDiv").style.display = "none";
				document.getElementById("SelfSettings")   .style.display = "none";
				document.getElementById("entireLog")   	  .style.display = "block";
				document.getElementById("entireLogB")  	  .style.display = "none";
				document.getElementById("entireSide")  	  .style.display = "none";
				
				document.getElementById("FireFox").style.backgroundColor = "#CCCCCC";
				document.getElementById("General").style.backgroundColor = "#CCCCCC";
				document.getElementById("Log")	  .style.backgroundColor = "#FFFF55";
				document.getElementById("LogB")	  .style.backgroundColor = "#CCCCCC";
				document.getElementById("Side")	  .style.backgroundColor = "#CCCCCC";
				
				if (Date.now() - HTTPUACleanerScripting.showTLSLogCount > 500)
				{
					self.port.emit
					(
						'showTLSLog',
						{TLSLogURL: TLSLogUrl}
					);

					HTTPUACleanerScripting.showTLSLogCount = Date.now();
				}

				document.getElementById("mainbody")	  .style.backgroundColor = "#C9C9C0";
			};
		}
		
		var logMouseOverFuncB = function(BLogUrl)
		{
			return function ()
			{
				document.getElementById("FireFoxSettings").style.display = "none";
				document.getElementById("CiphersSettings").style.display = "none";
				document.getElementById("ResetFireFox")   .style.display = "none";
				document.getElementById("CertificatesDiv").style.display = "none";
				document.getElementById("SelfSettings")   .style.display = "none";
				document.getElementById("entireLog")   	  .style.display = "none";
				document.getElementById("entireLogB")  	  .style.display = "block";
				document.getElementById("entireSide")  	  .style.display = "none";
				
				document.getElementById("FireFox").style.backgroundColor = "#CCCCCC";
				document.getElementById("General").style.backgroundColor = "#CCCCCC";
				document.getElementById("Log")	  .style.backgroundColor = "#CCCCCC";
				document.getElementById("LogB")	  .style.backgroundColor = "#FFFF55";
				document.getElementById("Side")	  .style.backgroundColor = "#CCCCCC";
				
				if (Date.now() - HTTPUACleanerScripting.showBLogCount > 500)
				{
					self.port.emit
					(
						'showBLog',
						{BLogURL: BLogUrl}
					);

					HTTPUACleanerScripting.showBLogCount = Date.now();
				}

				document.getElementById("mainbody")	  .style.backgroundColor = "#C9C9C0";
			};
		}
		
		var logMouseOverSide = function(SideLogUrl, lastUrls)
		{
			return function ()
			{
				document.getElementById("FireFoxSettings").style.display = "none";
				document.getElementById("CiphersSettings").style.display = "none";
				document.getElementById("ResetFireFox")   .style.display = "none";
				document.getElementById("CertificatesDiv").style.display = "none";
				document.getElementById("SelfSettings")   .style.display = "none";
				document.getElementById("entireLog")   	  .style.display = "none";
				document.getElementById("entireLogB")  	  .style.display = "none";
				document.getElementById("entireSide")  	  .style.display = "block";
				
				document.getElementById("FireFox").style.backgroundColor = "#CCCCCC";
				document.getElementById("General").style.backgroundColor = "#CCCCCC";
				document.getElementById("Log")	  .style.backgroundColor = "#CCCCCC";
				document.getElementById("LogB")	  .style.backgroundColor = "#CCCCCC";
				document.getElementById("Side")	  .style.backgroundColor = "#FFFF55";

				if (Date.now() - HTTPUACleanerScripting.showSLogCount > 500)
				{
					self.port.emit
					(
						'showSideLog',
						{SideLogURL: SideLogUrl, CurrentHost: lastUrls.CurrentHost, CurrentDomain: lastUrls.CurrentDomain}
					);
					
					HTTPUACleanerScripting.showSLogCount = Date.now();
				}

				document.getElementById("mainbody")	  .style.backgroundColor = "#C9C9C0";
			};
		}
		
		var isCurrentDomainKnown = false;
		if (options.HTTPUACleaner && options.HTTPUACleaner.currentHost && options.HTTPUACleaner.currentHost != "httpuseragentcleanerg-at-addons-dot-8vs-dot-ru")
		{
			HTTPUACleanerScripting.currentHost = options.HTTPUACleaner.currentHost;
			isCurrentDomainKnown = true;
		}
		else
		{
			HTTPUACleanerScripting.currentDomain = "";
			HTTPUACleanerScripting.currentHost   = "";
		}

		HTTPUACleanerScripting.currentDomain = options.HTTPUACleaner.currentDomain;//HTTPUACleanerScripting.currentDomain; //HTTPUACleanerScripting.getDomainByHost(HTTPUACleanerScripting.currentHost);
		if (HTTPUACleanerScripting.currentDomain == HTTPUACleanerScripting.currentHost)
			HTTPUACleanerScripting.currentHost = "." + HTTPUACleanerScripting.currentHost;

		document.getElementById("currentHost")   .textContent = options.HTTPUACleaner.currentHostToShow;
		//document.getElementById("currentHost2")  .textContent = options.HTTPUACleaner.currentHostToShow;
		document.getElementById("currentDomain") .textContent = options.HTTPUACleaner.currentDomainToShow;
		//document.getElementById("currentDomain2").textContent = HTTPUACleanerScripting.currentDomain;

		
		document.getElementById("Log").onmouseenter = logMouseOverFunc(options.TLS.TLSLogURL);
		HTTPUACleanerScripting.showTLSLogCount = 0;
		document.getElementById("LogB").onmouseenter = logMouseOverFuncB(options.TLS.bLogUrl);
		HTTPUACleanerScripting.showBLogCount = 0;
		if (options.TLS.loggerSideEnabled)
			document.getElementById("Side").onmouseenter = logMouseOverSide(options.TLS.sideLogUrl, options.TLS.lastUrls);

		HTTPUACleanerScripting.version = options.HTTPUACleaner.version;
		HTTPUACleanerScripting.mainOptionsCaptions = options.HTTPUACleaner.mainOptionsCaptions;

		var trustLevel  = (options.TLS.trustLevel.f     *100.0);
		var trustLevell = (options.TLS.trustLevel.long.f*100.0);
		if (trustLevel < 0)
			trustLevel = '----';
		else
		{
			trustLevel = '' + trustLevel;
			trustLevel = trustLevel.substring(0, 5) + '%';
		}
		if (trustLevell < 0)
			trustLevell = '----';
		else
		{
			trustLevell = '' + trustLevell;
			trustLevell = trustLevell.substring(0, 5) + '%';
		}
		
		if (options.TLS.trustLevel.f <= 0 && options.TLS.trustLevel.fa > 0)
		{
			trustLevell = '' + options.TLS.trustLevel.fa*100.0;
			trustLevell = '- / ' + trustLevell.substring(0, 5) + '%';
		}

		document.getElementById("TrustLevelDomain").textContent = HTTPUACleanerScripting.currentHost;
		document.getElementById("TrustLevel")      .textContent = trustLevel + ' / ' + trustLevell;
		document.getElementById("TrustLevelDiv")   .style['background-color'] = options.TLS.trustLevel.cl;

		var disableAllElement = document.getElementById("DisableAll");

		HTTPUACleanerScripting.setDisableAllColor(disableAllElement, options.HTTPUACleaner.enabled, options.HTTPUACleaner.mainOptionsColors);
		disableAllElement.onclick = HTTPUACleanerScripting.getDisableAllEvent(!options.HTTPUACleaner.enabled, options.HTTPUACleaner.mainOptionsColors);

		var names = options.HTTPUACleaner.mainOptionsNames;
		for (var index in names)
		{
			var namesPostfixs = ["", "Hold", "Domain", "Host"];
			for (var postfixIndex in namesPostfixs)
			{
				var name = names[index] + namesPostfixs[postfixIndex];
				var e = document.getElementById(name);
				if (!e) console.error(name + ' is not defined');

				var ebid = name + "Priority";
				var eb = document.getElementById(ebid);
				if (!eb)
				{
					eb = document.createElement("td");
					eb.id = ebid;
					e.parentNode.insertBefore(eb, e);
					eb.style.width = "20px";
					eb.style.textAlign = "center";
				}

				var state = options.eo[ebid];
				if (namesPostfixs[postfixIndex] == "" || namesPostfixs[postfixIndex] == "Hold")
				{
				}
				else
				if (options.eo[ebid])
				{
					if (namesPostfixs[postfixIndex] == "Domain")
						state = state[HTTPUACleanerScripting.currentDomain] || 5; //options.eo[names[index] + "Priority"];
					else
						state = state[HTTPUACleanerScripting.currentHost] || 5; //options.eo[names[index] + "Priority"];
				}
				else
					state = 5;

				HTTPUACleanerScripting.showColoredPriorityElement(  state, eb, options.HTTPUACleaner  );
				eb.onclick = HTTPUACleanerScripting.getOnClickPriorityElement(eb, options.HTTPUACleaner, {isCurrentDomainKnown: isCurrentDomainKnown, special: namesPostfixs[postfixIndex], name: names[index]});
			}
		}

		var names = options.HTTPUACleaner.mainOptionsNames;
		for (var index in names)
		{
			var name = names[index];
			var e = document.getElementById(name);

			HTTPUACleanerScripting.showColoredElement(options.eo[name], e, options.HTTPUACleaner);
			e.onclick = HTTPUACleanerScripting.getOnClickElement(e, options.HTTPUACleaner, {isCurrentDomainKnown: isCurrentDomainKnown, special: "main", name: name});

			var e = document.getElementById(name + "Hold")
			if (name != "MUA")
			{
				HTTPUACleanerScripting.showColoredElement(options.eo[name + "Hold"], e, options.HTTPUACleaner);

				e.onclick = HTTPUACleanerScripting.getOnClickElement(e, options.HTTPUACleaner, {isCurrentDomainKnown: isCurrentDomainKnown, special: "Hold", name: name});
			}
			else
				HTTPUACleanerScripting.showGrayElement(options.eo[name + "Hold"], e);

			var ed = document.getElementById(name + "Domain")
			var eh = document.getElementById(name + "Host")

			if (isCurrentDomainKnown)
			{
				var eod = options.eo[name + "Domain"];
				var optionsForDomain = eod[HTTPUACleanerScripting.currentDomain];
				var optionsForHost   = eod[HTTPUACleanerScripting.currentHost];

				if (optionsForDomain)
				{
					HTTPUACleanerScripting.showColoredElement(optionsForDomain, ed, options.HTTPUACleaner);
				}
				else
				{
					HTTPUACleanerScripting.showColoredElement('none', ed, options.HTTPUACleaner);
				}
				
				if (optionsForHost)
				{
					HTTPUACleanerScripting.showColoredElement(optionsForHost, eh, options.HTTPUACleaner);
				}
				else
				{
					HTTPUACleanerScripting.showColoredElement('none', eh, options.HTTPUACleaner);
				}
				
				ed.onclick = HTTPUACleanerScripting.getOnClickElement(ed, options.HTTPUACleaner, {isCurrentDomainKnown: isCurrentDomainKnown, special: "Domain", name: name});
				eh.onclick = HTTPUACleanerScripting.getOnClickElement(eh, options.HTTPUACleaner, {isCurrentDomainKnown: isCurrentDomainKnown, special: "Host", name: name});
			}
			else
			{
				HTTPUACleanerScripting.showColoredElement('none', ed, options.HTTPUACleaner);
				HTTPUACleanerScripting.showColoredElement('none', eh, options.HTTPUACleaner);
				
				ed.onclick = function() {};
				eh.onclick = function() {};
			}
		}

		HTTPUACleanerScripting.showCipherColoredElements(options.ciphers);
		HTTPUACleanerScripting.showOption1ColoredElements(options.options1);
		
		var resetElement = document.getElementById('ResetToDefaultFF');
		var val = HTTPUACleanerScripting.mainOptionsCaptions[resetElement.value];
		if (val)
			resetElement.value = val;
		resetElement.onclick = function()
					{
						self.port.emit
						(
							'ResetFF', {}
						);
					};
		
		resetElement = document.getElementById('ResetToDefaultEx');
		val = HTTPUACleanerScripting.mainOptionsCaptions[resetElement.value];
		if (val)
			resetElement.value = val;
		resetElement.onclick = function()
					{
						self.port.emit
						(
							'ResetEx', {}
						);
					};

		resetElement = document.getElementById('Certificates');
		val = HTTPUACleanerScripting.mainOptionsCaptions[resetElement.value];
		if (val)
			resetElement.value = val;
		resetElement.onclick = function()
					{
						self.port.emit
						(
							'Certificates', {}
						);
					};
		
		resetElement = document.getElementById('HttpLog');
		val = HTTPUACleanerScripting.mainOptionsCaptions[resetElement.value];
		if (val)
			resetElement.value = val;
		resetElement.onclick = function()
					{
						self.port.emit
						(
							'showHLog', {}
						);
					};
		
		resetElement = document.getElementById('ResetData');
		val = HTTPUACleanerScripting.mainOptionsCaptions[resetElement.value];
		if (val)
			resetElement.value = val;
		resetElement.onclick = function()
					{
						self.port.emit
						(
							'ResetData', {}
						);
					};
					
		resetElement = document.getElementById('ResetBLog');
		val = HTTPUACleanerScripting.mainOptionsCaptions[resetElement.value];
		if (val)
			resetElement.value = val;
		resetElement.onclick = function()
					{
						self.port.emit
						(
							'ResetBLog', {}
						);
					};
		/*
		resetElement = document.getElementById('ResetToDefaultSR');
		val = HTTPUACleanerScripting.mainOptionsCaptions[resetElement.value];
		if (val)
			resetElement.value = val;
		resetElement.onclick = function()
					{
						self.port.emit
						(
							'ResetSR', {}
						);
					};
		
		resetElement = document.getElementById('ResetToDefaultSC');
		val = HTTPUACleanerScripting.mainOptionsCaptions[resetElement.value];
		if (val)
			resetElement.value = val;
		resetElement.onclick = function()
					{
						self.port.emit
						(
							'ResetSC', {}
						);
					};
*/
	},	// конец getOptions - инициализации

		
	//   ----------------------------------------------------------------------------------
	//   -------------------- Ниже идут функции вкладки Side ------------------------------
	//   ----------------------------------------------------------------------------------

	showSideLog: function(SideTable)
	{
		var eSide = document.getElementById("entireSide");
		eSide.textContent = '';
		if (!SideTable)
			return;

		HTTPUACleaner_LogBView.setView(eSide, SideTable);
		document.documentElement.scrollTop = SideTable.scrool;
		
		if (HTTPUACleanerScripting.SideLogEventRegistered <= 0)
		{
			HTTPUACleanerScripting.SideLogEventRegistered = 1;

			self.port.on
				(
					'SideTableContent',
					function(data)
					{
						// Изменение условий правила
						var filtersChangeMain = function(data, e)
						{
							// Начинаем изменять условие правила
							if (data.squery[1] == 'begin')
							{
								if (data.html)
								{
									if (document.getElementById(data.html.id))
										return;

									e.textContent = '';
									var input = HTTPUACleaner_LogBView.setView(e, data.html);

									var ea = e;
									var onevent = function(args)
									{
										// F1
										if (args.keyCode == 112/* && args.shiftKey*/)
										{
											if (input.value.length > 0)
												input.value += ' | td[:]=' + data.host;
											else
												input.value = 'td[:]=' + data.host;

											if (args.preventDefault)
												args.preventDefault();
											args.cancelBubble = true;
											return;
										}
										// F2
										if (args.keyCode == 113/* && args.shiftKey*/)
										{
											if (input.value.length > 0)
												input.value += ' | td[:1]=' + data.domain;
											else
												input.value = 'td[:1]=' + data.domain;
											
											if (args.preventDefault)
												args.preventDefault();
											args.cancelBubble = true;
											return;
										}
										// F4
										if (args.keyCode == 115/* && args.shiftKey*/)
										{
											input.value = 'td[:]=' + input.value;

											if (args.preventDefault)
												args.preventDefault();
											args.cancelBubble = true;
											return;
										}

										// F9
										if (args.keyCode !== 120 && args.keyCode !== undefined && args.keyCode != 13)
											return;

										var ot = {};
										for (var nm in ea.data)
										{
											ot[nm] = ea.data[nm];
										}
										ot.etype = 'condition-change-main';
										ot.stype = ['change', 'changed'];
										
										if (args.keyCode == 120)
											ot.stype[1] = 'addSubRules';

										var o = 
										{
											query: 'mousedown',
											id:    data.id,
											id2:   input.id,
											text:  input.value,
											data:  ot
										};

										self.port.emit
										(
											'SideTab',
											o
										);

										return false;
									};

									input.addEventListener('blur',    onevent);
									input.addEventListener('keydown', onevent);
									input.focus();
								}

								return;
							}
							
							// Изменение условий правила закончено
							if (data.squery[1] == 'end')
							{
								var rm = document.getElementById(data.id2);
								rm.remove();
								e.textContent = data.text;
								return;
							}
						};
						
						var filtersChange = function(data, e)
						{
							if (data.squery[1] == 'end')
							{
								if (data.html)
								{
									e.textContent = '';
									HTTPUACleaner_LogBView.setView(e, data.html);
								}

								return;
							}
						};
						
						// Добавляем к правилу фильтры
						var filtersAdd = function(data, e)
						{
							if (data.squery[1] == 'show')
							{
								if (document.getElementById(data.html.id))
								{
									// document.getElementById(data.html.id).focus();
									return;
								}

								// Если список фильтров пуст - один элемент идёт пустой
								if (data.filters.length < 2)
								{
									e.textContent = '-';
									return;
								}
								
								e.textContent = '';
								var select = HTTPUACleaner_LogBView.setView(e, data.html);

								var ea = e;
								var onevent = function(args)
								{
									if (args.keyCode !== undefined && args.keyCode != 13)
										return;

									var ot = {};
									for (var nm in ea.data)
									{
										ot[nm] = ea.data[nm];
									}
									ot.etype = 'filter';
									ot.stype = ['change', 'changed'];

									var o = 
									{
										query: 'mousedown',
										id:    data.id,
										id2:   select.id,
										text:  select.value,
										data:  ot
									};

									self.port.emit
									(
										'SideTab',
										o
									);
								};
/*
								select.addEventListener('change',  onevent);*/
								select.addEventListener('blur',    onevent);
								select.addEventListener('keydown', onevent);
								select.focus();

								return;
							}
							
							if (data.squery[1] == 'end')
							{
								var rm = document.getElementById(data.id2);

								if (!rm)
								{
									console.error('HUAC soft error: not found ' + data.id2 + ' filter-add-select in Side tab of panel');
									console.error(data);
									
									e.textContent = '+';
									return;
								}

								rm.remove();
								e.textContent = '+';
								
								if (data.html)
								{
									var ef = document.getElementById(data.id3);
									if (!ef)
									{
										console.error('HUAC soft error: not found ' + data.id3 + ' filters in Side tab of panel');
										console.error(data);
										return;
									}

									ef.textContent = '';
									HTTPUACleaner_LogBView.setView(ef, data.html);
								}

								return;
							}
						};
						
						var filters = function(data)
						{
							/*
								data:
								query:   'filter',
								squery:  ['add', 'show'],
								id:      data.id,
								filters: filters
							*/
							
							var e = null;
							if (data.id)
							{
								e = document.getElementById(data.id);
								if (!e)
								{
									console.error('HUAC error: not found ' + data.id + ' (mousedown) in Side tab of panel');
									console.error(data);
									return;
								}
							}
							
							if (data.squery[0] == 'add')
							{
								filtersAdd(data, e);
							
								return;
							}
							
							if (data.squery[0] == 'change')
							{
								filtersChange(data, e);

								return;
							}
							
							if (data.squery[0] == 'change-main')
							{
								filtersChangeMain(data, e);

								return;
							}
						};
						
						if (data.query == 'filter')
						{
							filters(data);
							return;
						}
						
						var e = document.getElementById(data.id);
						if (!e)
						{
							console.error('HUAC error: not found ' + data.id + ' (mousedown) in Side tab of panel');
							console.error(data);
							return;
						}


						if (data.query == 'rm-confirm')
						{
							var a = 0;
							e.data['rm-confirma'] = document.defaultView.setTimeout
							(
								// Задержка для защиты от случайного удаления
								function()
								{
									// Это для того, чтобы уже неактуальное ожидание не влезло с изменением состояния
									// Т.к. может быть такое, что оно уже будет отменено к моменту истечения 2250 мс
									if (e.data['rm-confirma'] !== a)
										return;

									e.data['rm-confirm'] = true;
									e.textContent = 'X?';
								},
								2250
							);
							a = e.data['rm-confirma'];
							
							// Задержка для того, чтобы возможность удаления не висела вечно
							document.defaultView.setTimeout
							(
								function()
								{
									var e = document.getElementById(data.id);
									if (!e || e.data['rm-confirma'] !== a)
										return;

									e.data['rm-confirma'] = false;
									e.data['rm-confirm']  = false;
									e.textContent = 'X';
								},
								8000 + 2250
							);

							// Сигнализируем об ожидании
							// e.data['rm-confirm'] = 1;
							e.textContent = '!';
						}
						
						if (data.query == 'rm-unconfirm')
						{
							e.data['rm-confirma'] = false;
							e.data['rm-confirm']  = false;
							data.query = 'mousedown'
						}

						if (data.query == 'mousedown')
						{
							if (data.name && data.text !== null)
							e[data.name] = data.text;

							if (data.style)
							for (var ca in data.style)
							{
								e.style[ca] = data.style[ca];
							}
							
							return;
						}

						if (data.query == 'mousedown-change-name')
						{
							var input = document.getElementById(data.id + '-change');
							if (!input)
							    input   = document.createElement('input');
							else
							{
								// consore.error('already created');
								return;
							}

							input.type  = 'text';
							input.id    = data.id + '-change';
							input.data  = {number: data.number, type: 'rule', etype: 'name-change'};
							input.value = data.text;

							e.textContent = '';
							e.appendChild(input);
							
							var onevent = function(args)
							{
								if (args.keyCode == 112/* && args.shiftKey*/)
								{
									input.value += data.host;

									if (args.preventDefault)
										args.preventDefault();
									args.cancelBubble = true;
									return;
								}
								if (args.keyCode == 113/* && args.shiftKey*/)
								{
									input.value += data.domain;

									if (args.preventDefault)
										args.preventDefault();
									args.cancelBubble = true;
									return;
								}

								if (args.keyCode !== undefined && args.keyCode != 13)
									return;

								var ot = {};
								for (var nm in e.data)
								{
									ot[nm] = e.data[nm];
								}
								ot.etype = 'name-changed';

								var o = 
								{
									query: 'mousedown',
									id:    data.id,
									id2:   input.id,
									text:  input.value,
									data:  ot
								};


								self.port.emit
								(
									'SideTab',
									o
								);
							};

							// В случае нажатия Enter
							input.addEventListener
							(
								'keydown',
								onevent
							);
							
							// При потере фокуса
							input.addEventListener
							(
								'blur',
								onevent
							);

							input.focus();
							return;
						}
						
						if (data.query == 'remove')
						{
							e.remove();
							return;
						}
					}
				);
		}
	}
};


self.port.on
(
	'init',
	HTTPUACleanerScripting.getOptions
);


self.port.on
(
	'updateCiphers',
	HTTPUACleanerScripting.showCipherColoredElements
);

self.port.on
(
	'updateOptions1',
	HTTPUACleanerScripting.showOption1ColoredElements
);

self.port.on
(
	'setTLSLog',
	HTTPUACleanerScripting.showTLSLog
);

self.port.on
(
	'setBLog',
	HTTPUACleanerScripting.showBLog
);

self.port.on
(
	'setSideLog',
	HTTPUACleanerScripting.showSideLog
);

var body = document.getElementsByTagName("body")[0];
	body.oncontextmenu=function() {return false;};
