var getProtocolFromURL = function(URL, withoutDoublePoint)
{
	var protocol = '';
	try
	{
		protocol = /^([^:\/]+:)*([^:\/]+:)\/\//i.exec(URL);
		if (protocol == null)
			protocol = /^([^:\/]+:)*([^:\/]+:)/i.exec(URL);

		protocol = protocol[protocol.length - 1];
	}
	catch (e)
	{
		return "";
	}

	if (withoutDoublePoint)
		return protocol.substring(0, protocol.length - 1);

	return protocol;
};

var getProtocolForDocument = function(document, withoutDoublePoint)
{
	var protocol = '';
	if (document.location && document.location.protocol)
		protocol = document.location.protocol;
	else
		try
		{
			protocol = getProtocolFromURL(document.URL);
		}
		catch (e)
		{
			return "";
		}

	if (withoutDoublePoint)
		return protocol.substring(0, protocol.length - 1);

	return protocol;
};


var getPathByURI = function(URIString)
{
	if (!URIString)
		return '';

	var a = URIString.indexOf('://');
	if (a < 0)
	{
		if (URIString.startsWith('about:'))
		{
			var result = URIString.substring('about:'.length);
			if (result.indexOf(':') < 0)
				return result;
		}

		return '';
	}
	
	URIString = URIString.substr(a + 3);

	try
	{
		// file:/// , поэтому начинаем с [/]*
		// var regex = /^([^:\/]+:)*\/\/[^\/\\]+[\/\\]([^#?]+).*$/i;
		var regex = /^[^\/\\]*[\/\\]([^#?]+).*$/i;
		var result = regex.exec(URIString);

		if (!result)
			return "";

		result = result[1];

		if (!result)
			return "";
		else
			return result;
	}
	catch (exception)
	{
		return new String(URIString);
	}
};

var getHostByURI = function(URIString)
{
	if (!URIString)
		return "";

	var a = URIString.indexOf('://');
	if (a < 0)
	{
		return '';
	}
	
	// Если файл
	var fregex = /^([^:\/]+:)*file:\/\//i;
	if (fregex.test(URIString))
	{
		try
		{
			var regex = /^([^:\/]+:)*[^:\/]+:\/\/[\/]?([^\/:?#]+.*)$/i;
			var result = regex.exec(URIString);
			result = result[result.length - 1];

			var splitted = result.split("/");
			result = splitted[splitted.length - 1];
			for (var i = splitted.length - 1 - 1; i >= 0; i--)
			{
				result = splitted[i] + '/' + result;
				/*if (i > 1 && (result.length + splitted[0].length + splitted[1].length) > 56)
				{
					result = splitted[0] + '/' + splitted[1] + '/../' + result;
					break;
				}*/
			}

			if (!result)
				return '';
			else
				return '' + result;
		}
		catch (exception)
		{
			return new String(URIString);
		}

		return;
	}

	try
	{
		var regex = /^([^:\/]+:)*[^:\/]+:\/\/([\/]?[^\/:?#]+)[\/:?#]*/i;
		var result = regex.exec(URIString);

		result = result[result.length - 1];

		if (!result)
			return "";
		else
			return result;
	}
	catch (exception)
	{
		return new String(URIString);
	}
};

var getDomainByHost = function(host)
{
	if (!host)
		return "";

	// Если файл
	var fregex = /[\/:]/i;
	if (fregex.test(host))
	{
		try
		{
			var regex = /^\/*([^\/?#]+[\/?#]?[^\/?#]*)/i;
			var result = regex.exec(host);
			result = result[1];

			if (!result)
				return '';
			else
				return '' + result;
		}
		catch (exception)
		{
			return new String(host);
		}

		return;
	}

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
		return new String(host);
	}
};

var getURIWithoutScheme = function(URIString)
{
	if (!URIString)
		return "";

	try
	{
		var regex = /^[^:\/]+:\/\/(.*)/i;
		var result = regex.exec(URIString);

		result = result[1];

		if (!result)
			return "";
		else
			return result;
	}
	catch (exception)
	{
		return new String(URIString);
	}
};

var split = function(uriString, splitChars, stripEmpty, maxL, noSeparator, start)
{
	var result = [];

	if (start)
		uriString = uriString.substr(start);

	var found = 0;
	do
	{
		var min = uriString.length;
		var a   = '';
		for (var sc of splitChars)
		{
			var i = uriString.indexOf(sc, found);
			if (i >= 0 && i < min)
			{
				min = i;
				a   = sc;
			}
		}
		
		if (min < uriString.length)
		{
			if (!stripEmpty || min != 0)
				result.push(uriString.substr(0, min));

			if (!noSeparator)
				result.push(uriString.substr(min, a.length))

			uriString = uriString.substr(min + a.length);
		}
		else
		{
			result.push(uriString);
			return result;
		}

		if (maxL && maxL > 0 && result.length >= maxL)
		{
			result.push(uriString);
			return result;
		}

	}
	while (uriString.length > 0);

	return result;
};

var toArray = function(uriString)
{
	var uri  = uriString.replace(/\\/g, '/');
	var uria = uri.indexOf('?');
	if (uria > 0)
	{
		var a = uri.substring(0, uria).split('/');
		if (a.length == 1 && a[0] == '')
			a = [];

		a.splice(0, 0, uri.substring(uria));
		return a;
	}
	else
	{
		var a = uri.split('/');
		if (a.length == 1 && a[0] == '')
			a = [];

		// a.splice(0, 0, '');
		return a;
	}
};

var toArrayPoint = function(uriString)
{
	var uri  = uriString.replace(/\\/g, '/');
	var uria = uri.indexOf(':');
	if (uria > 0)
	{
		var a = uri.substring(0, uria).split('.');
		a.push(uri.substring(uria));
		return a.reverse();
	}
	else
	{
		var a = uri.split('.');
		a.push('');
		return a.reverse();
	}
};


var arrayPathToCanonical = function(uriArray)
{
	for (var i = 0; i < uriArray.length; i++)
	{
		if (!uriArray[i] || uriArray[i] == '.')
		{
			uriArray.splice(i, 1);
			i--;
			continue;
		}

		if (i > 0 && uriArray[i] == '..' && uriArray[i - 1] != '..')
		{
			uriArray.splice(i - 1, 2);
			i -= 2;
			continue;
		}
	}
};

var concatArrayPath = function(uriArray1, uriArray2)
{
	var result = [];
	for (var i = 0; i < uriArray1.length; i++)
		result.push(uriArray1[i]);
	for (var i = 0; i < uriArray2.length; i++)
		result.push(uriArray2[i]);

	return this.arrayPathToCanonical(result);
};

var joinArrayPath = function(uriArray, count)
{
	if (!uriArray || uriArray.length <= 0)
		return '';
	
	if (!count)
		count = uriArray.length;
	
	var str = uriArray[0];
	for (var i = 1; i < uriArray.length && i < count; i++)
		if (uriArray[i][0] == '?' && i + 1 == uriArray.length)
			str += uriArray[i];
		else
			str += '/' + uriArray[i];
	
	return str;
};

var joinArrayPathPoint = function(uriArray, count)
{
	if (!uriArray || uriArray.length <= 0)
		return '';
	
	if (!count)
		count = uriArray.length;
	
	var i0 = uriArray.length - 1;
	var str = uriArray[i0--];
	if (str[0] == ':')
	{
		str = uriArray[i0--];
	}

	for (var i = i0; i >= 0 && count > 0; i--, count--)
			str = uriArray[i] + '.' + str;
	
	return str;
};


var RandomStr = function(len)
{
	var CLD = 0;
	var c   = len + this.getRandomInt(1, (len>>3) + (Date.now() % (len>>3)));

	var A   = new Array(c);
	for (var  i = 0; i < c; i++)
	{
		if (i % 7 == 0)
			A[i] = String.fromCharCode(this.getRandomIntShift(48, 57+1, CLD));
		else
		if (i % 7 < 4)
			A[i] = String.fromCharCode(this.getRandomIntShift(97, 122+1, CLD));
		else
			A[i] = String.fromCharCode(this.getRandomIntShift(65, 90+1, CLD));
	}

	this.shuffle3(A, this.getRandomInt(c >> 3, c >> 1));

	return A.join("");
};

var getRandomInt = function(min, max)
{
	return Math.floor(Math.random() * (max - min)) + min;
};

var getRandomIntShift = function(min, max, shift)
{
	return (Math.floor(Math.random() * (max - min)) + shift) % (max - min) + min;
};

var shuffle3 = function(a, c)
{
	var l = a.length;
	var t, t1, t2, t3, c1, c2, c3;

	for (var i = 0; i < c; i++)
	{
		c1 = this.getRandomInt(0, l - 1);
		c2 = this.getRandomInt(0, l - 1);
		c3 = this.getRandomInt(0, l - 1);

		if (c1 == c2 || c1 == c3 || c2 == c3)
		{
			if (c1 == c2)
			{
				t = a[c1];
				a[c1] = a[c3];
				a[c3] = t;
			}
			else
			{
				var t = a[c1];
				a[c1] = a[c2];
				a[c2] = t;
			}
		}
		else
		{
			t1 = a[c1];
			t2 = a[c2];
			t3 = a[c3];
			a[c1] = t3;
			a[c2] = t1;
			a[c3] = t2;
		}
	}
};

var getRandomValueByArray = function(array, length)
{
	if (!length)
		length = array.length;

	return array[this.getRandomInt(0, length)];
};

var getRandomValueByArrayFreq = function(array, freq)
{
	var length = 0;
	
	for (var i = 0; i < freq.length; i++)
		length += freq[i];

	var num = this.getRandomInt(1, length + 1);
	
	var index = 0;
	length = 0;
	for (; index < array.length; index++)
	{
		length += freq[index];
		if (length >= num)
		{
			break;
		}
	}
	if (index >= array.length)
		index = array.length - 1;

	return array[index];
};


var truncateString = function(str, count)
{
	if (!count || str.length <= count)
		return str;
	
	return str.substring(0, count) + '...';
};



EXPORTED_SYMBOLS = ['getProtocolForDocument', 'getProtocolFromURL', 'getPathByURI', 'getHostByURI', 'getDomainByHost', 'getURIWithoutScheme', 'split', 'toArray', 'toArrayPoint', 'arrayPathToCanonical', 'concatArrayPath', 'joinArrayPath', 'joinArrayPathPoint', 'RandomStr', 'getRandomInt', 'getRandomIntShift', 'shuffle3', 'getRandomValueByArray', 'getRandomValueByArrayFreq', 'truncateString'];
