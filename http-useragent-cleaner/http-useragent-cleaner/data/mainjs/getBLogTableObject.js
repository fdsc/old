// HTTPUACleaner.getTLSLogTableObject(HTTPUACleaner.getTLSLogObject(activeTabUrl));

HTTPUACleaner.getBLogTableObject = function(urlString, logger, DLstr, opts)
{
	if (!opts)
		opts = {NO_OPTS: true, cached: true, notcached: true, validated: true, TLS: true, notTLS: true, qTLS: true};

	if (!logger)
		logger = HTTPUACleaner.loggerB;

	if (!logger.enabled)
	{
		return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('Blocking log disabled by user')};
	}

	var DL  = DLstr;
	var noTab = false;
	if (!DLstr)
	{
		if (urlString.length > 0)
			DL = HTTPUACleaner['sdk/preferences/service'].get(HTTPUACleaner_Prefix + 'logb.displayedLevels', '0 2 3 6 7 9 10 11');
		else
		{
			DL = HTTPUACleaner['sdk/preferences/service'].get(HTTPUACleaner_Prefix + 'logb.displayedLevelsNoTab', '0 2 3 4 5 6 7 9 10 11');
			noTab = true;
		}
	}

	var urls = HTTPUACleaner.urls;
	
	DL = urls.split(DL, [',', ' ', '.', ';'], true, 0, true);

	var tabInfo = null;
	if (!urlString || urlString == 'about:newtab' || urlString == 'about:blank' || urlString.indexOf('about:neterror') >= 0 || urlString == '')
		tabInfo = logger.getEmptyTabs(true);
	else
		tabInfo = logger.FindTab(urlString);

	if (tabInfo == null)
	{
		// Если записи о такой вкладке нет, возможно речь идёт о заблокированном запросе, который записан на пустую вкладку
		tabInfo = logger.getEmptyTabs(true);
		if (tabInfo == null)
		{
			if (opts.NO_OPTS !== true)
				return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('Have no http requests')};
			else
				return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('Have no blocking')};
		}
	}

	if (!tabInfo.BlockInfo)
	{
		if (opts.NO_OPTS !== true)
			return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('Have no http requests')};
		else
			return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('Have no blocking')};
	}

	var add = function(parent, child)
	{
		if (!parent.html)
			parent.html = [];
		
		parent.html.push(child);
	};

	var create = function(tagName, parent)
	{
		var r = {tag: tagName, style: {/*'word-wrap': 'break-word', */'overflow-wrap': 'break-word'}};
		
		if (parent)
			add(parent, r);
		
		return r;
	};
	
	var font = function(node, fontName)
	{
		if (!node.style)
			node.style = {};
		
		node.style['font-family'] = fontName;
	};
	
	var text = function(node, text, truncCount, noDecode)
	{
		if (text === true)
			text = 'YES';
		else
		if (text === false)
			text = 'NO';

		if (!text || !text.length)
			text = '' + text;

		if (!noDecode)
		try
		{
			text = decodeURI(text);
		}
		catch (e)
		{}
		
		node.textContent = urls.truncateString(text, truncCount);
	};
	
	var time = function(node, time, msgLevel)
	{
		var to2 = function(str)
		{
			str = str.toString();
			
			if (str.length > 1)
				return str;
			
			return '0' + str;
		};
		
		var t   = new Date(time);
		var str = to2(t.getHours()) + ':' + to2(t.getMinutes()) + ':' + to2(t.getSeconds());
		if (msgLevel || msgLevel === 0)
			str = str + ' (' + msgLevel + ')';

		node.textContent = str;
	};
	
			
	var colSpan = opts.shortLog ? 3 : 2;


	var aColor = ['#448844', '#777777', '#FF8800', '#FF0000', '#000088', '#0000FF', '#FFBB00', '#888800', '#AAAAAA', '#FFAA88', '#FF4422',
				/* 11 */ "#FFFFFF"];
	var tColor = ['#000000', '#000000', '#000000', '#000000', '#FFFFFF', '#FFFFFF', '#000000', '#000000', '#000000', '#000000', '#000000', 
					'#000000'];
	var shortLogColor;
	var getTR = function(table, msg, add)
	{
		var tr  = create('tr', table);
		var td1 = create('td', tr);
		var td2 = create('td', tr);
		font(td1, 'Courier New');
		font(td2, 'Courier New');

		if (opts.shortLog)
		{
			time(td1, msg.time);
			if (msg.serviceData.isPrivateChannel)
			{
				td1.style['background-color'] = '#000000';
				td1.style['color'] = '#FFFFFF';
			}

			var ptd2 = create('p', td2);
			text(ptd2, msg.serviceData.method + ' ' + msg.url);
			td2 .absmaxwidth = 55;
			ptd2.absmaxwidth = 55;
			ptd2.style['overflow'] = 'hidden';
			ptd2.style['white-space'] = 'nowrap';

			var td3  = create('td', tr);
			var ptd3 = create('p', td3);
			/*td3 .absmaxwidth = 40;
			ptd3.absmaxwidth = 40;*/
			ptd3.style['overflow'] = 'hidden';
			ptd3.style['white-space'] = 'nowrap';
			text(ptd3, msg.msg.tab);

			tr  = create('tr', table);
			td1 = create('td', tr);
			td2 = create('td', tr);
			td3 = create('td', tr);

			if (!msg.serviceData.stopped)
				tr.style['background-color'] = '#00FF00AA';

			var cnttype = msg.serviceData['content-type'];
			if (!cnttype)
				cnttype = '';

			var rsp = '';
			if (msg.serviceData.responseStatus)
				rsp = msg.serviceData.responseStatus + ' ' + msg.serviceData.responseStatusText + ' ';

			var isCached = false;
			if (msg.serviceData.transferSize && msg.serviceData.cached)
				isCached = true;

			text(td2, rsp + '[' +  msg.msg.received + (isCached ? ' (cached)' : '') + ']' + (msg.serviceData.statusText ? ' '+ msg.serviceData.statusText : ''), undefined, false);
			text(td3, cnttype, 80, false);

			
			if (!msg.serviceData.stopped)
			{
				var cancelButton = create('img', td1);
				cancelButton.id  = 'cancel-' + msg.serviceData.channelId;
				cancelButton.src = 'data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAABKSURBVChThU8JDgAgCMI+1P+/VQ8wKe1YWzGZBKVLFLByKKR3WV5CRnV9g5lyREZhO0jPxHL2Sx6Sacx64HodnCv2MEScLft8E2iJOEsXe/pHuQAAAABJRU5ErkJggq5CYII=';
			}

			var td1Text      = create('span', td1);
			if (msg.serviceData.transferSize)
				text(td1Text, msg.serviceData.transferSize ? msg.serviceData.transferSize : '', undefined, false);
			else
			if (msg.serviceData.cached)
				text(td1Text, 'cached', undefined, false);

			if (msg.serviceData.canceled)
			{
				if (msg.serviceData.redirected)
					tr.style['background-color'] = '#FFFF0033';
				else
				if (msg.serviceData.responseStatus == 304 || msg.serviceData.cached)
					tr.style['background-color'] = '#FFFF0088';
				else
					tr.style['background-color'] = '#FFFF00';
				
				shortLogColor = tr.style['background-color'];
			}
			else
			if (!msg.serviceData.redirected && msg.serviceData.statusText)
			{
				tr.style['background-color'] = '#FF0000';
				shortLogColor = tr.style['background-color'];
			}
			else
			if (msg.serviceData.responseStatus >= 400)
			{
				tr.style['background-color'] = '#FF000088';
				shortLogColor = tr.style['background-color'];
			}
			else
			{
				if (msg.serviceData.isPrivateChannel)
					shortLogColor = '#000000AA';
				else
					shortLogColor = aColor[11];
			}

			return;
		}

		time(td1, msg.time, msg.level);
		td1.style['min-width'] = '20%'
		text(td2, msg.type, 80);

		var isSideType = msg.type == 'side';

		if (aColor[msg.level] && msg.level != 1)
		{
			td1.style['background-color'] = aColor[msg.level];
			td1.style['color'] = tColor[msg.level];
		}

		var func = function(recordName, record, color, bgcolor, noDecode, maxLen, img)
		{
			tr  = create('tr', table);
			td1 = create('td', tr);
			td2 = create('td',	tr);
			font(td1, 'Courier New');
			font(td2, 'Courier New');
			
			if (recordName != '__canvasdata')
			{
				text(td1, recordName, 40);
				//td1.style['min-width'] = '2em'
				td1.absmaxwidth = 16;
				var p = create('p', td2);
				// p.style['max-width'] = '60em';
				p.absmaxwidth = 75;
				text(p, record, (maxLen ? maxLen : 85*8), noDecode);

				if (color)
				{
					if (!td2.style)
						td2.style = {};
					
					td2.style.color = color;
					p  .style.color = color;
				}
				if (bgcolor)
				{
					if (!td2.style)
						td2.style = {};

					td2.style['background-color'] = bgcolor;
					p  .style['background-color'] = bgcolor;
				}

				if (img)
				{
					create('br',	td2);
					var imgE = create('img',	td2);
					imgE.src = img;
					imgE.absmaxwidth = 75;
					imgE.absmaxWheight = 75;
				}
			}
			else
			{
				if (record && record.width && record.height && record.width * record.height <= 640*480)
				{
					if (record.data)
					{
						text(td1, 'canvas image', 20);

						var canvas = create('canvas', td2);
						canvas['__canvasdata'] = {};
						canvas['__canvasdata'].width  = record.width;
						canvas['__canvasdata'].height = record.height;
						canvas['__canvasdata'].data   = {};

						for (var a in record.data)
						{
							canvas['__canvasdata'].data[a] = record.data[a];
						}
					}
				}

				if (record)
					func('canvas', '' + record.width + 'x' + record.height);
				else
					func('canvas', 'error');
			}
		};

		for (var record in msg.msg)
		{
			// Фильтры отрезает, кажется
			if (HTTPUACleaner.truncLenght.side && !Number.isNaN(Number(record)))
			{
				console.error();
				continue;
			}

			var recordVal = msg.msg[record];
			if (!recordVal || recordVal.indexOf instanceof Function || (!recordVal.name && !recordVal.val))
				func(record, msg.msg[record]);
			else
			{
				func(recordVal.name, recordVal.val, recordVal.color, recordVal.bgcolor, recordVal.noDecode, recordVal.maxLen, recordVal.img);
			}
		}

		if (add)
		{
			for (var adda of add)
			{
				func(adda.name, adda.val);
			}
		}

		if (msg.repeatedCount)
		{
			tr  = create('tr', table);
			
			var td = create('td', tr);
			font(td, 'Courier New');
			text(td, '+' + msg.repeatedCount);
			td.colSpan = colSpan;
		}
	};


	table = create('table');
	table.style.width = '100%';

	if (tabInfo.truncated)
	{
		var trt = create('tr', table);
		var tdt = create('td', trt);
		tdt.colSpan = colSpan;
		tdt.textContent = 'Truncated ' + tabInfo.tcount + ' records';
	}

	var objectsEqual = function(a, bi, j)
	{
		if (bi.length <= j)
			return false;

		var b = bi[j];

		if (!a.msg || !b.msg || !a.msg.source || !b.msg.source || !b.time || !a.time || b.time - a.time > 5 * 1000 || b.repeatedCount || a.repeatedCount)
			return false;

		var ac = JSON.parse(JSON.stringify(a));
		var bc = JSON.parse(JSON.stringify(b)); // Object.create не работает - исключает из объекта вообще все поля
		delete ac.msg.source;
		delete bc.msg.source;
		delete ac.time;
		delete bc.time;
		delete ac.repeatedCount;
		delete bc.repeatedCount;
		delete ac.msg.ft;
		delete bc.msg.ft;
		delete ac.msg.ct;
		delete bc.msg.ct;

		var acs = JSON.stringify(ac);
		var bcs = JSON.stringify(bc);

		if (acs == bcs)
			return {source: b.msg.source, ct: b.msg.ct};

		return false;
	};


	var regex  = null;
	var regexn = null;
	var regexh = null;

	if (opts.urlf)
	{
		try
		{
			regex = new RegExp(opts.urlf,    'i');
		}
		catch (e)
		{
			console.error('HUAC EXCLAMATION: In the YOU url regex have error');
			console.error(e.message);
		}
	}

	if (opts.headern)
	{
		try
		{
			regexn = new RegExp(opts.headern, 'i');
		}
		catch (e)
		{
			console.error('HUAC EXCLAMATION: In the YOU header name regex have error');
			console.error(e.message);
		}
	}

	if (opts.headerf)
	{
		try
		{
			regexh = new RegExp(opts.headerf, 'i');
		}
		catch (e)
		{
			console.error('HUAC EXCLAMATION: In the YOU header value regex have error');
			console.error(e.message);
		}
	}

	var cntToLog = 0;
	//for (var a of tabInfo.BlockInfo)
	for (var i = 0; i < tabInfo.BlockInfo.length; i++)
	{
		var a = tabInfo.BlockInfo[i];
		//if (a.level != 3)
		//if (a.level != 2 || a.level == 2 && noTab)

		if (DL.indexOf(a.level.toString()) < 0)
			continue;

		if (a.serviceData)
		{
			if (opts.errors)
			{
				if (
					a.serviceData.status == 0 || a.serviceData.redirected
				// || a.serviceData.responseStatus && a.serviceData.responseStatus == 304 && a.serviceData.status == 0x80540005
					)
				if (!a.serviceData.responseStatus || a.serviceData.responseStatus < 400)
					continue;
			}

			if (opts.notGet)
			{
				if (a.serviceData.method == 'GET')
					continue;
			}

			if (!opts.cached)
			{
				if (a.serviceData.cached && !a.serviceData.validated)
					continue;
			}
			
			if (!opts.notcached)
			{
				if (!a.serviceData.cached && !a.serviceData.validated)
					continue;
			}

			if (!opts.validated)
			{
				if (a.serviceData.validated)
					continue;
			}
			
			if (!opts.TLS)
			{
				if (a.serviceData.TLS === true)
					continue;
			}
			
			if (!opts.notTLS)
			{
				if (a.serviceData.TLS === false)
					continue;
			}
			
			if (!opts.qTLS)
			{
				if (a.serviceData.TLS !== true && a.serviceData.TLS !== false)
					continue;
			}

			if (!opts.completed)
			{
				if (a.serviceData.stopped)
					continue;
			}

			if (!opts.notCompleted)
			{
				if (!a.serviceData.stopped)
					continue;
			}

			// https://developer.mozilla.org/ru/docs/Web/JavaScript/Reference/Global_Objects/RegExp
			if (opts.headern || opts.headerf)
			{
				var testPair = function(name, value)
				{
					try
					{
						if (regexn)
						{
							var testResult = regexn.test(name);
							if (opts.invertn)
							{
								if (testResult)
									return false;
							}
							else
							{
								if (!testResult)
								{
									return false;
								}
							}
						}

						if (regexh)
						{
							if (opts.invertf)
							{
								if (regexh.test(value))
									return false;
							}
							else
							{
								if (!regexh.test(value))
								{
									return false;
								}
							}
						}
					}
					catch (e)
					{
						console.error('HUAC WARNING: in the http log regex for the headers has error');
						console.error(e);
					}

					return true;
				};


				var headerFlag = false;
				for (var h in a.serviceData.headers.pre)
				{
					if (testPair(h, a.serviceData.headers.pre[h]))
					{
						headerFlag = true;
						break;
					}
				}

				for (var h in a.serviceData.headers.r)
				{
					if (testPair(h, a.serviceData.headers.r[h]))
					{
						headerFlag = true;
						break;
					}
				}

				for (var h in a.serviceData.headers.resp)
				{
					var ha = a.serviceData.headers.resp[h];

					if (ha)
					if (testPair(ha[0], ha[1]))
					{
						headerFlag = true;
						break;
					}
				}

				if (headerFlag ^ !opts.inverth)
				{
					continue;
				}
			}

			if (opts.urlf)
			{
				try
				{
					if (opts.inverturl)
					{
						if (regex.test(a.serviceData.url))
							continue;
					}
					else
					{
						if (!regex.test(a.serviceData.url))
							continue;
					}
				}
				catch (e)
				{
					console.error('HUAC WARNING: in the http log regex for the url has error');
					console.error(e);
				}
			}
		}

		cntToLog++;
		var tr = create('tr', table);
		var td = create('td', tr);

		td.colSpan = colSpan;
		td.style = {};
		td.style.height = '0.1em';
		td.style['background-color'] = '#000000'
		td.style['font-family'] = 'Courier New';
		td.absmaxwidth = 91;
		
		var cnt = table.html.length;
		var tra = tr;
		var adda = undefined;
		if (opts.shortLog)
		{
			
		}
		else
		{
			tr = create('tr', table);
			td = create('td', tr);
			td.colSpan = colSpan;
			td.style['font-family'] = 'Courier New';
			td.absmaxwidth = 91;
			text(td, a.url, HTTPUACleaner.truncLenght.url);

			var addai = 0;
			do
			{
				// Пропускаем элементы, которые не отображаются, т.к. в противном случае
				// между одинаковыми type='side' встанет type='HTTP request', а он, обычно, не отображается
				while (tabInfo.BlockInfo.length > i+1 && DL.indexOf(tabInfo.BlockInfo[i+1].level.toString()) < 0)
					i++;

				var b = objectsEqual(a, tabInfo.BlockInfo, i + 1);

				if (b)
				{
					i++;
					addai++;

					if (!adda)
						adda = [];

					var p = {};
					p.name = 'source ' + addai;
					p.val = b.source;

					adda.push(p);
					
					if (b.ct)
					{
						p = {};
						p.name = 'ct ' + addai;
						p.val  = b.ct;
						
						adda.push(p);
					}
				}
				else
					break;
			}
			while (true);
		}

		getTR(table, a, adda);

		td = create('td');
		tra.html.splice(0, 0, td);

		td.rowSpan = table.html.length - cnt + 1;
		td.style = {};
		td.style.width = '0.1em';
		td.style['background-color'] = '#000000';

		if (aColor[a.level])
		{
			td.style['background-color'] = aColor[a.level];
			td.style['color'] = tColor[a.level];
		}

		if (shortLogColor)
		{
			td.style['background-color'] = shortLogColor;
		}
	}
	
	if (cntToLog <= 0)
	{
		if (opts.NO_OPTS !== true)
			return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('Have no http requests')};
		else
			return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('Have no blocking')};
	}

	return table;
};
