HTTPUACleaner.browsers =
[
	// http://relaxtime.8vs.ru/navigator.html

	// http://www.useragentstring.com/pages/FireFox/
	// 0
	{
		getUA:      function()
					  {
						var result = 
						{
							"userAgent":	"Mozilla/5.0 ($oscpu; rv:$rv) Gecko/$productSub Firefox/$rv",
							"appCodeName":	"Mozilla",
							"appName":		"Netscape",
							"appVersion":	"5.0",
							"oscpu":		"",	
							"platform":		"",
							"product":		"",
							"productSub":	"",
							"vendor":		"",
							"vendorSub":	"",
							appMinorVersion:null,
							cpuClass:		null,
							buildID:		null,
							browserLanguage: null
						};
						
						var os = HTTPUACleaner.getOSIdentifiers();
						
						result.oscpu 	= os.oscpu;
						result.platform = os.platform;
						
						result.userAgent = result.userAgent.replace("$oscpu", os.osString);

						if (os.oscpu.indexOf("Windows") == 0)
						{
							result.appVersion += " (Windows)";
						}
						else
						if (os.oscpu.indexOf("X11") == 0)
						{
							result.appVersion += " (X11)";
						}
						else
						if (os.oscpu.indexOf("Macintosh") == 0)
						{
							result.appVersion += " (Macintosh)";	// как на самом деле - я не знаю
						}

						result.product 	  = "Gecko";

						result.productSub = "20100101";/*HTTPUACleaner.getRandomInt(2011, 2013+1).toString() + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(1, 12+1).toString()) + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(1, 28+1).toString());*/
						
						// 20130730113002
						result.buildID = result.productSub + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(0, 23+1).toString()) + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(0, 59+1).toString()) + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(0, 59+1).toString());

						result.userAgent = result.userAgent.replace("$productSub", result.productSub);
						// https://wiki.mozilla.org/RapidRelease/Calendar
						// /g - иначе только первое вхождение заменит
						var rv = ['17.0', '24.0', '38.0', '45.0', '46.0', '47.0', '48.0', '49.0', '50.0', '51.0', '52.0'];
						result.userAgent = result.userAgent.replace(/\$rv/g, HTTPUACleaner.getRandomValueByArray(rv));


						return result;
					  }
	},
	
	// http://www.useragentstring.com/Opera12.14_id_19612.php
	// 1
	{
		getUA:      function()
					  {
						var result = 
						{
							"userAgent":	"Opera/9.80 ($oscpu; en) Presto/$rp Version/$rv",
							"appCodeName":	"Mozilla",
							"appName":		"Opera",
							"appVersion":	"$rv ($oscpu)",
							browserLanguage: "en",
							"oscpu":		null,	
							"platform":		null,
							"product":		null,
							"productSub":	null,
							"vendor":		null,
							"vendorSub":	null,
							appMinorVersion:"",
							cpuClass:		null,
							buildID:		null
						};
						
						var os = HTTPUACleaner.getOSIdentifiers();
						
						result.oscpu 	= os.oscpu;
						result.platform = os.platform;
						
						result.userAgent = result.userAgent.replace("$oscpu", os.osString);

						result.appVersion = result.appVersion.replace("$oscpu", os.osString);
						
						
						var rv  = ['12.17', "12.14", "12.02", "12.00", "11.62", "11.52"];
						var rp  = {'12.17': '2.12.388', "12.14": "2.12.388", "12.02": "2.10.289", "12.00": "2.9.181", "11.62": "2.10.229", "11.52": "2.9.168"};
						var rva = HTTPUACleaner.getRandomValueByArray(rv);
						// /g - иначе только первое вхождение заменит
						result.userAgent = result.userAgent.replace(/\$rv/g, rva);
						result.userAgent = result.userAgent.replace(/\$rp/g, rp[rva]);

						result.appVersion = result.appVersion.replace("$rv", rva);

						return result;
					  }
	},

	// Opera >= 23.0
	// 2
	{
		getUA:      function()
					  {
						var result = 
						{
							"userAgent":	'Mozilla/5.0 ($platform) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.101 Safari/537.36 OPR/25.0.1614.50 (Edition Campaign 67)',
							"appCodeName":	"Mozilla",
							"appName":		"Netscape",
							"appVersion":	'5.0 ($platform) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.101 Safari/537.36 OPR/25.0.1614.50 (Edition Campaign 67)',
							browserLanguage: null,
							language: 		'en',
							maxTouchPoints: HTTPUACleaner.getRandomValueByArray([0, 1]),		// 26 - maxTouchPoints: 0
							hardwareConcurrency: HTTPUACleaner.getRandomValueByArray([2, 4, 8]),
							"oscpu":		null,
							"platform":		null,
							"product":		"Gecko",
							"productSub":	'20030107',
							"vendor":		'Opera Software ASA',
							"vendorSub":	'',
							appMinorVersion:null,
							cpuClass:		null,
							buildID:		null
						};
						
						var os = HTTPUACleaner.getOSIdentifiers();
						
						//result.oscpu 	= os.oscpu;
						result.platform = os.platform;

						var rv  = [
									/*'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36 OPR/23.0.1522.77',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.68 Safari/537.36 OPR/24.0.1558.3',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.101 Safari/537.36 OPR/25.0.1614.50 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.111 Safari/537.36 OPR/25.0.1614.68',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36 OPR/26.0.1656.24',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36 OPR/26.0.1656.60 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.91 Safari/537.36 OPR/27.0.1689.54 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.94 Safari/537.36 OPR/27.0.1689.66',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.94 Safari/537.36 OPR/27.0.1689.66 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36 OPR/27.0.1689.69 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36 OPR/27.0.1689.69',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.115 Safari/537.36 OPR/27.0.1689.76 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.115 Safari/537.36 OPR/27.0.1689.76',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.76 Safari/537.36 OPR/28.0.1750.40 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.76 Safari/537.36 OPR/28.0.1750.40',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.89 Safari/537.36 OPR/28.0.1750.48 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.89 Safari/537.36 OPR/28.0.1750.48',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36 OPR/28.0.1750.51 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36 OPR/28.0.1750.51',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36 OPR/29.0.1795.47 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36 OPR/29.0.1795.47',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.152 Safari/537.36 OPR/29.0.1795.60 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.152 Safari/537.36 OPR/29.0.1795.60',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36 OPR/30.0.1835.59 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36 OPR/30.0.1835.59',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.125 Safari/537.36 OPR/30.0.1835.88 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.125 Safari/537.36 OPR/30.0.1835.88',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.130 Safari/537.36 OPR/30.0.1835.125 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.130 Safari/537.36 OPR/30.0.1835.125',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36 OPR/32.0.1948.25 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36 OPR/32.0.1948.25',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.93 Safari/537.36 OPR/32.0.1948.69 (Edition Campaign 67)',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.93 Safari/537.36 OPR/32.0.1948.69',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36 OPR/33.0.1990.58',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.73 Safari/537.36 OPR/34.0.2036.25',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.109 Safari/537.36 OPR/35.0.2066.68',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36 OPR/35.0.2066.82',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.75 Safari/537.36 OPR/36.0.2130.32',*/
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.87 Safari/537.36 OPR/37.0.2178.32',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.63 Safari/537.36 OPR/38.0.2220.29',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.84 Safari/537.36 OPR/38.0.2220.31',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 OPR/39.0.2256.71',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.116 Safari/537.36 OPR/40.0.2308.81',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36 OPR/40.0.2308.90',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36 OPR/41.0.2353.46',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36 OPR/42.0.2393.137',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36 OPR/42.0.2393.517',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36 OPR/43.0.2442.1144',
									'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.78 Safari/537.36 OPR/47.0.2631.55'
									];
						
						//getRandomValueByArrayFreq

						arv = [];
						for (var i = 0; i < rv.length; i++)
							arv.push(i + 1);

						var rva = HTTPUACleaner.getRandomValueByArrayFreq(rv, arv);
						if (rva.indexOf('OPR/2') >= 0)
						{}
						else
							result.vendor = 'Google Inc.';

						// /g - иначе только первое вхождение заменит
						var platform = '5.0 ($platform) '.replace(/\$platform/g, os.platform);
						rva = platform + rva;
						result.userAgent  = 'Mozilla/' + rva;

						result.appVersion = rva; //rva.replace("$platform", os.platform);

						return result;
					  }
	},
	
	// http://www.useragentstring.com/pages/Chrome/
	// 3
	{
		getUA:      function()
					  {
						var result = 
						{
							"userAgent":	"Mozilla/5.0 ($oscpu) $productEnum",
							"appCodeName":	"Mozilla",
							"appName":		"Netscape",
							"appVersion":	"5.0",
							"oscpu":		"",
							"platform":		"",
							"product":		"",
							"productSub":	"",
							"vendor":		"Google Inc.",
							"vendorSub":	"",
							appMinorVersion:null,
							cpuClass:		null,
							buildID:		null,
							browserLanguage: null
						};

						var os = HTTPUACleaner.getOSIdentifiers();
						
						//result.oscpu 	= os.oscpu;
						result.platform = os.platform;
						
						result.userAgent = result.userAgent.replace("$oscpu", os.osString);
						
						/*
						if (os.oscpu.indexOf("Windows") == 0)
						{
							result.appVersion += " (Windows)";
						}
						else
						if (os.oscpu.indexOf("X11") == 0)
						{
							result.appVersion += " (X11)";
						}
						else
						if (os.oscpu.indexOf("Macintosh") == 0)
						{
							result.appVersion += " (Macintosh)";	// как на самом деле - я не знаю
						}*/

						result.product 	  = "Gecko";

						// http://www.webapps-online.com/online-tools/user-agent-strings/dv/browser51853/chrome
						var productEnum = 
						[
							/*"AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.60 Safari/537.17",
							"AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17",
							"AppleWebKit/537.15 (KHTML, like Gecko) Chrome/24.0.1295.0 Safari/537.15",
							"AppleWebKit/537.14 (KHTML, like Gecko) Chrome/24.0.1292.0 Safari/537.14",
							"AppleWebKit/537.13 (KHTML, like Gecko) Chrome/24.0.1290.1 Safari/537.13",
							"AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1468.0 Safari/537.36",
							"AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1464.0 Safari/537.36",
							"AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36",
							"AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36",
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.38 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1636.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.4 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1659.2 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.146 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1788.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1859.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1918.1 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.1990.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2064.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2127.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2174.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2215.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2273.0 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.152 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.93 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.86 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.73 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.87 Safari/537.36',*/
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.84 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.75 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.76 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36',
							'AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36'
						];
						// http://browserss.ru/google-chrome.html

						arv = [];
						for (var i = 0; i < productEnum.length; i++)
							arv.push(i + 1);

						var rva = HTTPUACleaner.getRandomValueByArrayFreq(productEnum, arv);
						
						/*
						var r = /AppleWebKit\/([0-9]+\.[0-9]+)\s/;
						var r = r.exec(productEnum);
						result.productSub = r[1];
						*/
						result.userAgent = result.userAgent.replace("$productEnum", rva);
						
						result.appVersion = result.userAgent.substring(result.appCodeName.length);
						
						result.productSub = HTTPUACleaner.getRandomInt(2008, 2011 + 1).toString() + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(1, 12 + 1).toString()) + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(1, 28 + 1).toString());
						
						return result;
					  }
	},
	
	// http://www.useragentstring.com/pages/Internet%20Explorer/
	// 4
	{
		getUA:      function()
					{
						var result = 
						{
							"userAgent":	"Mozilla/",
							"appCodeName":	"Mozilla",
							"appName":		"Microsoft Internet Explorer",
							"appVersion":	"",
							"appMinorVersion": "0",
							"cpuClass":     "x86",
							oscpu:			null,
							vendor:			null,
							vendorSub:		null,
							product:		null,
							productSub:		null,
							buildID:		null,
							browserLanguage: null
						};
						
						var appVersion = "5.0 ($compatible; MSIE $IEVersion; $oscpu$Trident$advanced)";
						var version = HTTPUACleaner.getRandomValueByArray
						(
						[
							"11",
							"10.6",
							"10.0",
							"9.0",
							"8.0",
							"7.0"
						]
						);
						appVersion = appVersion.replace("$IEVersion", version);

						var os = HTTPUACleaner.getOSIdentifiers();
						
						// result.oscpu 	= os.oscpu;
						result.platform = os.platform;
						
						var trident = HTTPUACleaner.getRandomValueByArray
						(
						[
							"",
							"; Trident/6.0",
							"; Trident/5.0",
							"; Trident/4.0"
						]
						);
						["Windows; U", "X11", "compatible"]
						
						var lunascape = HTTPUACleaner.getRandomValueByArray
						(
							["", "", "", "", "", "", "", "", "", "", "", "",
							"; Lunascape 6.6.0.25173", "; Lunascape/6.7.1.25446", "; Lunascape/6.4.5.23569"]
						);
						
						var advanced	= function()
						{
							var str = "";
							for (var i = 0; i < HTTPUACleaner.getRandomInt(0, 6); i++)
								str += "; " + 
									HTTPUACleaner.getRandomValueByArray
									(
									[
										"InfoPath.3",
										"InfoPath.2",
										"SLCC1",
										"SLCC2",
										"SV1",
										"Media Center PC 6.0",
										".NET CLR 3.0.4506.2152",
										".NET CLR 3.5.30729",
										".NET CLR 2.0.50727",
										".NET4.0C",
										".NET4.0E",
										"MS-RTC LM 8",
										"Zune 4.7",
										"Zune 4.0",
										"chromeframe/12.0.742.112",
										"Tablet PC 2.0",
										"Win64",
										"x64"
									]
									);

							return str;
						}
						
						var compatible = HTTPUACleaner.getRandomInt(0, 10);

						if (compatible == 0)
						{
							if (os.oscpu.indexOf("Windows") == 0)
							{
								appVersion = appVersion.replace("$compatible", "Windows; U");
							}
							else
							if (os.oscpu.indexOf("X11") == 0)
							{
								appVersion = appVersion.replace("$compatible", "X11");
							}
							else
							if (os.oscpu.indexOf("Macintosh") == 0)
							{
								appVersion = appVersion.replace("$compatible", "Macintosh");
							}
							else
								appVersion = appVersion.replace("$compatible", "compatible");
						}
						else
							appVersion = appVersion.replace("$compatible", "compatible");
						
						appVersion = appVersion.replace("$oscpu",   os.osString);
						appVersion = appVersion.replace("$Trident", trident);
						appVersion = appVersion.replace("$advanced", advanced() + lunascape);
						
						
						result.appVersion = appVersion;
						result.userAgent += appVersion;
						
						return result;
					}
	},

	// http://www.useragentstring.com/pages/Safari/
	// 5
	{
		getUA:      function()
					  {
						var result = 
						{
							"userAgent":	"Mozilla/5.0 ($oscpu) $productEnum",
							"appCodeName":	"Mozilla",
							"appName":		"Netscape",
							"appVersion":	"5.0",
							"oscpu":		"",	
							"platform":		"",
							"product":		"",
							"productSub":	"",
							"vendor":		"Apple Computer, Inc.",
							"vendorSub":	"",
							appMinorVersion:null,
							cpuClass:		null,
							buildID:		null,
							browserLanguage: null
						};
						
						var os = HTTPUACleaner.getOSIdentifiers();
						
						//result.oscpu 	= os.oscpu;
						result.platform = os.platform;
						
						result.userAgent = result.userAgent.replace("$oscpu", os.osString);

						result.product 	  = "Gecko";

						var productEnum = HTTPUACleaner.getRandomValueByArray
						(
						[
							"AppleWebKit/537.13+ (KHTML, like Gecko) Version/5.1.7 Safari/534.57.2",
							"AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25",
							"AppleWebKit/534.57.2 (KHTML, like Gecko) Version/5.1.7 Safari/534.57.2",
							"AppleWebKit/534.55.3 (KHTML, like Gecko) Version/5.1.3 Safari/534.53.10",
							"AppleWebKit/534.46 (KHTML, like Gecko ) Version/5.1 Mobile/9B176 Safari/7534.48.3",
							"AppleWebKit/533.3 (KHTML, like Gecko) Lunascape/6.4.2.23236 Safari/533.3",
							"AppleWebKit/533.3 (KHTML, like Gecko) Lunascape/6.3.4.23051 Safari/533.3",
							"AppleWebKit/533.3 (KHTML, like Gecko) Lunascape/6.1.0.20995 Safari/533.3",
							"AppleWebKit/533.21.1 (KHTML, like Gecko) Version/5.0.5 Safari/533.21.1",
							"AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27",
							"AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/34.0.0.0 Mobile Safari/537.36"
						]
						);

						result.userAgent = result.userAgent.replace("$productEnum", productEnum);
						
						result.appVersion = result.userAgent.substring(result.appCodeName.length);
						
						result.productSub = HTTPUACleaner.getRandomInt(2003, 2011 + 1).toString() + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(1, 12 + 1).toString()) + HTTPUACleaner.toTwoLengthString(HTTPUACleaner.getRandomInt(1, 28 + 1).toString());
						
						return result;
					  }
	}
];


HTTPUACleaner.getOSIdentifiers = function(isLow)
{
	var length = HTTPUACleaner.OSIdentifiers.length;
	return HTTPUACleaner.getRandomValueByArray(HTTPUACleaner.OSIdentifiers);
};


// http://marsbeach.com/webalizer/agent_201701.html
// http://marsbeach.com/webalizer/usage_201702.html#TOPAGENTS
HTTPUACleaner.OSIdentifiers =
[
	{osString: "Windows NT 6.2;  Win64; x64", oscpu: "Windows NT 6.2; WOW64", platform: "Win64"},
	{osString: "Windows NT 6.1;  Win64; x64", oscpu: "Windows NT 6.2; WOW64", platform: "Win64"},
	{osString: "Windows NT 6.0;  Win64; x64", oscpu: "Windows NT 6.2; WOW64", platform: "Win64"},

	// двойной повтор x64
	{osString: "Windows NT 6.2;  Win64; x64", oscpu: "Windows NT 6.2; WOW64", platform: "Win64"},
	{osString: "Windows NT 6.1;  Win64; x64", oscpu: "Windows NT 6.2; WOW64", platform: "Win64"},

	// Строки Windows
	{osString: "Windows NT 10.0; Win64; x64", oscpu: "Windows NT 10.0; Win64; x64", 		platform: "Win64"},
	{osString: "Windows NT 10.0; WOW64",oscpu: "Windows NT 10.0; WOW64",platform: "Win64"},
	{osString: "Windows NT 9.0; en-US", oscpu: "Windows NT 9.0", 		platform: "Win64"},
	{osString: "Windows NT 7.1; en-US", oscpu: "Windows NT 7.1", 		platform: "Win64"},
	{osString: "Windows NT 6.3; Win64; x64", oscpu: "Windows NT 6.3; Win64; x64", 		platform: "Win32"},
	{osString: "Windows NT 6.3", 		oscpu: "Windows NT 6.3", 		platform: "Win32"},
	{osString: "Windows NT 6.2; WOW64", oscpu: "Windows NT 6.2; WOW64", platform: "Win32"},
	{osString: "Windows NT 6.2", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},
	{osString: "Windows NT 6.1; WOW64", oscpu: "Windows NT 6.2; WOW64", platform: "Win32"},	// эта строка взята из реального браузера
	{osString: "Windows NT 6.1", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},
	{osString: "Windows NT 6.0; WOW64", oscpu: "Windows NT 6.2; WOW64", platform: "Win32"},
	{osString: "Windows NT 6.0", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},
	{osString: "Windows NT 5.2; WOW64", oscpu: "Windows NT 6.2; WOW64", platform: "Win32"},
	{osString: "Windows NT 5.2", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},
	{osString: "Windows NT 5.1", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},

	
	// дублирование Windows для более частого появления
	
	{osString: "Windows NT 10.0; Win64; x64", oscpu: "Windows NT 10.0; Win64; x64", 		platform: "Win64"},
	{osString: "Windows NT 10.0; WOW64",oscpu: "Windows NT 10.0; WOW64",platform: "Win64"},
	{osString: "Windows NT 9.0; en-US", oscpu: "Windows NT 9.0", 		platform: "Win32"},
	{osString: "Windows NT 7.1; en-US", oscpu: "Windows NT 7.1", 		platform: "Win32"},
	{osString: "Windows NT 6.2; WOW64", oscpu: "Windows NT 6.2; WOW64", platform: "Win32"},
	{osString: "Windows NT 6.2", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},
	{osString: "Windows NT 6.1; WOW64", oscpu: "Windows NT 6.2", 		platform: "Win32"},
	{osString: "Windows NT 6.1", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},

	{osString: "Windows NT 10.0; Win64; x64", oscpu: "Windows NT 10.0; Win64; x64", 		platform: "Win64"},
	{osString: "Windows NT 10.0; WOW64",oscpu: "Windows NT 10.0; WOW64",platform: "Win64"},
	{osString: "Windows NT 9.0; en-US", oscpu: "Windows NT 9.0", 		platform: "Win32"},
	{osString: "Windows NT 7.1; en-US", oscpu: "Windows NT 7.1", 		platform: "Win32"},
	{osString: "Windows NT 6.2; WOW64", oscpu: "Windows NT 6.2; WOW64", platform: "Win32"},
	{osString: "Windows NT 6.2", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},
	{osString: "Windows NT 6.1; WOW64", oscpu: "Windows NT 6.2", 		platform: "Win32"},
	{osString: "Windows NT 6.1", 		oscpu: "Windows NT 6.2", 		platform: "Win32"},
	// ---------------
	
	
	{osString: "X11; Linux x86_64", 				oscpu: "Linux x86_64",		platform: "Linux x86_64"},
	{osString: "X11; Linux i686", 					oscpu: "Linux i686",		platform: "Linux i686"},		// эта строка ??зята из реального браузера
	{osString: "X11; FreeBSD amd64", 				oscpu: "FreeBSD amd64",		platform: "FreeBSD amd64"},
	{osString: "X11; FreeBSD i386", 				oscpu: "FreeBSD i386",		platform: "FreeBSD i386"},		

	{osString: "Macintosh; Intel Mac OS X 10_12_2",	oscpu: "Intel Mac OS X",	platform: "Intel Mac OS X"},
	{osString: "Macintosh; Intel Mac OS X 10.12", 	oscpu: "Intel Mac OS X",	platform: "Intel Mac OS X"},
	{osString: "Macintosh; Intel Mac OS X 10_8_3",	oscpu: "Intel Mac OS X",	platform: "Intel Mac OS X"},
	{osString: "X11; CrOS i686 8872.73.0", 			oscpu: "CrOS i686",			platform: "CrOS i686"},
	{osString: "X11; CrOS x86_64 8743.83.0", 		oscpu: "CrOS i686",			platform: "WinCrOS i68632"},

	{osString: "Windows; U; Windows NT 5.2; en-US", oscpu: "Windows NT 5.2",	platform: "Win32"},
	{osString: "Windows; U; Windows NT 5.0; en-US", oscpu: "Windows NT 5.0",	platform: "Win32"}
];


HTTPUACleaner.generateRandomUA = function(isLowRandom, statem)
{
	if (statem != "disabled")
	{
		if (statem == "Firefox 28")
		{
			var result = 
					{
						"userAgent":	"Mozilla/5.0 (X11; OpenBSD amd64; rv:50.0) Gecko/20100101 Firefox/50.0",
						// Mozilla/5.0 (X11; Linux x86_64; rv:28.0) Gecko/20100101  Firefox/28.0
						// Mozilla/5.0 (Windows NT 6.1; WOW64; rv:28.0) Gecko/20100101  Firefox/28.0
						"appCodeName":	"Mozilla",
						"appName":		"Netscape",
						"appVersion":	"5.0 (X11)",
						"oscpu":		"OpenBSD amd64",
						"platform":		"X11",
						"product":		"Gecko",
						"productSub":	"20100101",
						"vendor":		"",
						"vendorSub":	"",
						appMinorVersion:null,
						cpuClass:		null,
						buildID:		"20140314220517",
						browserLanguage: null
					};
			return result;
		}

		if (statem == "Opera 12.14")
		{
			var result = 
					{
						"userAgent":	"Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36 OPR/40.0.2308.90",
						"appCodeName":	"Mozilla",
						"appName":		"Opera",
						"appVersion":	"9.80 (X11; Linux x86_64)",
						"oscpu":		"Linux x86_64",
						"platform":		"X11",
						"product":		null,
						"productSub":	null,
						"vendor":		"Google Inc.",
						"vendorSub":	null,
						appMinorVersion:null,
						cpuClass:		null,
						buildID:		null,
						browserLanguage: "en"
					};
			return result;
		}
		
		if (statem == "Chrome 33")
		{
			var result = 
					{
						"userAgent":	"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
						"appCodeName":	"Mozilla",
						"appName":		"Netscape",
						"appVersion":	"5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
						"oscpu":		null,
						"platform":		"Win32",
						"product":		null,
						"productSub":	"20030107",
						"vendor":		"Google Inc.",
						"vendorSub":	null,
						appMinorVersion:null,
						cpuClass:		"x86",
						buildID:		null,
						browserLanguage: null
					};
			return result;
		}
		
		if (statem == "IE 10.0")
		{
			var result = 
					{
						"userAgent":	"Mozilla/5.0 (compatible; MSIE 11.0; Windows NT 6.2; WOW64; Trident/6.0; .NET4.0E; .NET4.0C)",
						"appCodeName":	"Mozilla",
						"appName":		"Microsoft Internet Explorer",
						"appVersion":	"5.0 (compatible; MSIE 11.0; Windows NT 6.2; WOW64; Trident/6.0; .NET4.0E; .NET4.0C)",
						"cpuClass": 	"x86",
						"platform":		"Win32",
						"product":		null,
						"productSub":	"20030107",
						"vendor":		"Microsoft",
						"vendorSub":	null,
						appMinorVersion:null,
						oscpu:			null,
						buildID:		null,
						browserLanguage: null
					};
			return result;
		}

		if (statem == "Googlebot")
		{
			var result = 
					{
						"userAgent":	"Googlebot/2.1 (+http://www.google.com/bot.html)",
						"appCodeName":	null,
						"appName":		null,
						"appVersion":	null,
						"oscpu":		null,
						"platform":		null,
						"product":		null,
						"productSub":	null,
						"vendor":		null,
						"vendorSub":	null,
						appMinorVersion:null,
						cpuClass:		null,
						buildID:		null,
						browserLanguage: null
					};
			return result;
		}
	}

	var browser = isLowRandom ? HTTPUACleaner.browsers[0] : HTTPUACleaner.getRandomValueByArrayFreq(HTTPUACleaner.browsers, [20, 0, 80, 80, 0, 0]);
	var navigator = browser.getUA();
	
	return navigator;
};
