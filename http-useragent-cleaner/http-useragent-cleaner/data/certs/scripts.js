var eventFunc = function(args)
{
	var request = args.request;
	var data    = args.data;
	
	if (!request || !(request instanceof Array))
	{
		console.error('HUAC ERROR: eventFunc in certs/scripts.js, no request');
		return;
	}
	
	if (request[0] == 'certs')
	{
		if (request[1] == 'table')
		{
			document.getElementById('mainbody').textContent = '';
			view.setView(document.getElementById('mainbody'), data, 'cert');
		}
	}
	else
	if (request[0] == 'hosts')
	{
		if (request[1] == 'table')
		{
			document.getElementById('mainbody').textContent = '';
			view.setView(document.getElementById('mainbody'), data, 'certHosts');
		}
	}
	else
	if (request[0] == 'failures')
	{
		if (request[1] == 'table')
		{
			document.getElementById('mainbody').textContent = '';
			view.setView(document.getElementById('mainbody'), data, 'certFailure');
		}
	}
};

self.port.on("cert", eventFunc);
self.port.on("certHosts", eventFunc);
self.port.on("certFailure", eventFunc);
