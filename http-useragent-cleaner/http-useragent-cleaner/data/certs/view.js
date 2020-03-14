view = {};

view.setView = function(eSide, BTable, message)
{
	if (!BTable)
		return null;

	var c = view.createHTML(BTable, message);
	eSide.appendChild(c);

	return c;
};

view.createHTML = function(bi, message)
{
	if (!bi)
		return null;

	var result = document.createElement(bi.tag);

	for (var c in bi)
	{
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
					{/* иначе текст выдел€етс€ по двойному щелчку мыши
						arg.cancelBubble = true;
						arg.returnValue  = false;
						arg.stopPropagation();*/
						if (!result['data']['mousedown-noPreventDefault'])
							arg.preventDefault();

						if (result['data'].unlock)
						{
							var e = document.getElementById(result['data'].unlock);
							if (e)
								e.disabled = !e.disabled;
						}

						if (result['data']['grayScreen'])
						{
							var gray = document.createElement('div');
							gray.style['background-color'] = 'rgba(0,0,0,0.7)';
							gray.style['width']  	= '100%';
							gray.style['height'] 	= '100%';
							gray.style['z-index'] 	= '1';
							gray.style['position'] 	= 'fixed';
							gray.style['top'] 		= '0';
							gray.style['left'] 		= '0';

							document.getElementById('mainbody').appendChild(gray);
						}


						if (result.id)
						document.defaultView.setTimeout
						(	function()
							{
								self.port.emit
								(
									message,
									{
										request: ['mousedown'],
										data:    {
													dt: result.data,
													name: result.name,
													id: result.id,
													checked: document.getElementById(result.id).checked
												 },
									}
								);
							},
							// ≈сли поставить ноль, то какие-то гонки начинаютс€, т.к. серый экран то показыватес€, то нет
							50
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
			result.appendChild(view.createHTML(subElement, message));
		}
	}

	return result;
};
