HTTPUACleaner_LogBView = {};

HTTPUACleaner_LogBView.setView = function(eSide, BTable)
{
	if (!BTable)
		return null;

	var c = HTTPUACleaner_LogBView.createHTML(BTable);
	eSide.appendChild(c);

	return c;
};

HTTPUACleaner_LogBView.createHTML = function(bi)
{
	if (!bi)
		return null;

	var result = document.createElement(bi.tag);

	for (var c in bi)
	{
		if (c == '__canvasdata' && bi.tag == 'canvas')
		{
			result.width  = bi[c].width;
			result.height = bi[c].height;
			ctx = result.getContext("2d");

			var dt = ctx.getImageData(0, 0, result.width, result.height);
			for (var a in dt.data)
			{
				try
				{
					dt.data[a] = bi[c].data[a];
				}
				catch (e)
				{
					console.error(e);
					break;
				}
			}

			ctx.putImageData(dt, 0, 0);
		}
		else
		if (c == 'absmaxwidth' || c == 'absmaxWheight')
		{
			if (c == 'absmaxwidth')
				result['style']['max-width'] = bi[c]*document.documentElement.clientWidth/100; //window.innerWidth/100;
			else
			if (c == 'absmaxWheight')
				result['style']['max-height'] = bi[c]*document.documentElement.clientWidth/100; // Высота зависит от ширины
		}
		else
		if (c != 'tag' && c != 'style' && c != 'data')
			result[c] = bi[c];
		else
		if (c == 'style')
		{
			for (var ca in bi[c])
			{
				result[c][ca] = bi[c][ca];
			}
		}
		else
		if (c == 'data')
		{
			if (!result[c])
				result[c] = {};

			var dataBI = bi[c];
			for (var ca in bi[c])
			{
				result[c][ca] = dataBI[ca];
			}

			if (result[c]['mousedown'] === true)
				result.addEventListener
				(
					'mousedown',
					function(arg)
					{/* иначе текст выделяется по двойному щелчку мыши
						arg.cancelBubble = true;
						arg.returnValue  = false;
						arg.stopPropagation();*/
						if (!result['data']['mousedown-noPreventDefault'])
							arg.preventDefault();

						self.port.emit
						(
							'SideTab',
							{
								query: 'mousedown',
								data:  result.data, //document.getElementById(bi['id']).data,
								id:    bi['id'],
								shift: arg.shiftKey,
								crtl:  arg.ctrlKey,
								scrool:document.documentElement.scrollTop
							}
						);

						return false;
					}
				);
		}
	}

	if (bi.html && bi.html.length > 0)
	{
		for (var subElement of bi.html)
		{
			result.appendChild(HTTPUACleaner_LogBView.createHTML(subElement));
		}
	}

	return result;
};
