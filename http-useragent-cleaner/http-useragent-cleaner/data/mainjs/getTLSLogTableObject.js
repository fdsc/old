HTTPUACleaner.getTLSLogObject = function(urlString)
{
	// var tabInfo = HTTPUACleaner.logger.FindTab(urls.fromTabDocument(urls.sdkTabToChromeTab(urls.tabs.activeTab)));
	var tabInfo = HTTPUACleaner.logger.FindTab(urlString);
	if (tabInfo == null)
		return false;

	var result = {
					TLS:[], truncated: tabInfo.truncated, tcount: tabInfo.tcount//, urlString: urlString
				};

	var TLSInfo = tabInfo.TLSInfo;

	for (var ti in TLSInfo)
	{
		var t = TLSInfo[ti];
/*
		if (t.error)
		{
			var info = {url: t.url, contentType: t.contentType, error: error};
			continue;
		}
		*/
		var a  = HTTPUACleaner.calcTrustLevelColor(t.f);
		var al = HTTPUACleaner.calcTrustLevelColor(t.flong);

		var info = {url: t.url, contentType: t.contentType, remoteAddress: t.remoteAddress, f: t.f, cl: a[1], fl: t.flong, cll: al[1], msg: []};
		result.TLS.push(info);
		
		if (t.msg)
		for (var msg of t.msg)
		{
			info.msg.push(msg);
		}

		if (t.certMsg)
		for (var msg of t.certMsg)
		{
			for (var msga of msg.msg)
				info.msg.push(msga);
		}
	}


	return result;
};

// HTTPUACleaner.getTLSLogTableObject(HTTPUACleaner.getTLSLogObject(activeTabUrl));
HTTPUACleaner.getTLSLogTableObject = function(TLSLogObject)
{
	if (!HTTPUACleaner.EstimateTLS && (!TLSLogObject || TLSLogObject.TLS.length <= 0))
	{
		return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('TLS log disabled by user')};
	}

	if (!TLSLogObject || TLSLogObject.TLS.length <= 0)
		return {tag: 'div', textContent: HTTPUACleaner['sdk/l10n'].get('No information')};;

	var urls = HTTPUACleaner.urls;
	var l10n = HTTPUACleaner['sdk/l10n'].get;

	// Иначе очень медленно работает
	var strs = 
	{
		'Signature': '',
		'Signature hash': '',
		'Signature and signature hash': '',
		'Symmetric cipher key length': '',
		'Symmetric cipher': '',
		'MAC for symmetric cryptography': '',
		'Key exchange': '',
		'TLS Version': '',
		'NO': '',
		'YES': '',
		'Truncated': '',
		'records': '',
		'Age (steal)': '',
		'HPKP lifetime': '',
		'HPKP certificate': '',
		'Check not performed': '',
		'Unknown log': '',
		'INVALID': ''
	};
	
	for (var stra in strs)
	{
		strs[stra] = l10n(stra);
	}
	strs['Signature hash (not use because root)'] = l10n('Signature hash A') + ' (' + l10n('not use because root') + ')';

	var _  = function(str)
	{
		if (strs[str])
			return strs[str];
		
		return str;
	};

	var add = function(parent, child)
	{
		if (!parent.html)
			parent.html = [];
		
		parent.html.push(child);
	};
	
	var create = function(tagName, parent)
	{
		var r = {tag: tagName};
		
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
	
	var text = function(node, text, truncCount)
	{
		if (text === true)
			text = _('YES');
		else
		if (text === false)
			text = _('NO');
		
		if (!text.length)
			text = '' + text;

		node.textContent = urls.truncateString(_(text), truncCount);
	};
	
	var getTR = function(table, msg)
	{
		var a  = HTTPUACleaner.calcTrustLevelColor(msg.f, false, msg.middleColor, msg.fHPKP);
		var al = HTTPUACleaner.calcTrustLevelColor(msg.flong, false, msg.middleColor, msg.fHPKP);

		var fs  = (msg.f*100.0)    .toString().substring(0, 5);
		var fls = (msg.flong*100.0).toString().substring(0, 5);

		if (msg.f < 0.00001)
			fs = '0.000';
		if (msg.flong < 0.00001)
			fls = '0.000';

		tr = create('tr', table);
		td = create('td', tr);
		text(td, msg.msg, 64);

		td = create('td', tr);
		text(td, msg.value, 32);
		
		td = create('td', tr);
		td.style = {'background-color': '#' + a[1], 'color': '#' + a[2]};
		text(td, fs, 12);
		
		td = create('td', tr);
		td.style = {'background-color': '#' + al[1], 'color': '#' + a[2]};
		text(td, fls, 12);
		
		return tr;
	};

	table = create('table');
	table.style = {width: '100%'};
	
	var tr, td;
	if (TLSLogObject.truncated)
	{
		tr = create('tr', table);
		td = create('td', tr);
		td.colSpan = 4;
		td.textContent = _('Truncated') + ' ' + TLSLogObject.tcount + ' ' +  _('records');
	}
	
	for (var a of TLSLogObject.TLS)
	{
		var fs  = (a.f *100).toString().substring(0, 5);
		var fls = (a.fl*100).toString().substring(0, 5);

		if (a.f < 0.00001)
			fs = '0.000';
		if (a.fl < 0.00001)
			fls = '0.000';

		// Если давалась оценка (то есть даже если оценка 0.0, вся функция оценки прошла)
		if (a.msg.length > 0)
		{
			tr = create('tr', table);
			td = create('td', tr);
			td.colSpan = 4;
			td.style = {height: '0.5em', 'background-color': '#777777'};
			
			tr = create('tr', table);
			td = create('td', tr);
			font(td, 'Courier New');
			text(td, a.contentType, 48);
			td.colSpan = 1;
			td = create('td', tr);
			font(td, 'Courier New');
			text(td, a.remoteAddress, 20);
			td.colSpan = 1;
			
			td = create('td', tr);
			td.style = {'background-color': '#' + a.cl};
			font(td, 'Courier New');
			text(td, fs, 64);
			td.rowSpan = 2;
			
			td = create('td', tr);
			td.style = {'background-color': '#' + a.cll};
			font(td, 'Courier New');
			text(td, fls, 64);
			td.rowSpan = 2;
			
			tr = create('tr', table);
			td = create('td', tr);
			font(td, 'Courier New');
			text(td, a.url, 80);
			td.colSpan = 2;
		}
		else
		{
			tr = create('tr', table);
			td = create('td', tr);
			td.colSpan = 4;
			td.style = {height: '0.5em', 'background-color': '#777777'};
			
			tr = create('tr', table);
			td = create('td', tr);
			font(td, 'Courier New');
			text(td, a.contentType, 48);
			td.colSpan = 1;
			td = create('td', tr);
			font(td, 'Courier New');
			text(td, a.remoteAddress, 20);
			td.colSpan = 1;
			td = create('td', tr);
			td.colSpan = 2;
			
			tr = create('tr', table);
			td = create('td', tr);
			font(td, 'Courier New');
			text(td, a.url, 80);
			td.colSpan = 2;
			
			td = create('td', tr);
			td.style = {'background-color': '#' + a.cl};
			font(td, 'Courier New');
			text(td, fs, 64);
			td.rowSpan = 1;
			
			td = create('td', tr);
			td.style = {'background-color': '#' + a.cll, left: 0};
			font(td, 'Courier New');
			text(td, fls, 64);
			td.rowSpan = 1;
		}

		for (var msg of a.msg)
		{
			getTR(table, msg);
		}
	}

	return table;
};
