self.port.on
(
	'blockedUrl',
	function(opt)
	{
		if (!opt.url)
		{
			self.port.emit("blockedUrl", {url: document.location.search.substr(1)});
			return;
		}

		var url = document.getElementById('url');
		var a = document.getElementById('aurl');
		url.textContent = opt.url;
		a.textContent = opt.url;
		a.href = opt.url;
	}
);
