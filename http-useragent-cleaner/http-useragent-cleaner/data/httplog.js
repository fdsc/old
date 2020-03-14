var UdjtNpJK2wcK = {};

self.port.on
(
	'setHLog',
	function(opts)
	{
		var BTable    = opts.BTable;
		var hlenabled = opts.hlenabled;

		var hle = document.getElementById("hlenabled");
		if (hlenabled === true)
		{
			hle.checked = true;
		}
		else
		{
			hle.checked = false;
		}

		if (!BTable)
		{
			document.getElementById("entireLogB").textContent = '';
			return;
		}

		var st = window.scrollY;
		document.getElementById("refresh").disabled = true;

		var gray = document.getElementById("grayScreen");
		if (!document.getElementById("shortLog").checked)
		{
			//gray.style['z-index'] = '2';
			gray.style['display'] = 'block';
		}

		if (!UdjtNpJK2wcK.noFirst)
		{
			document.getElementById("entireLogB").textContent = '';
			UdjtNpJK2wcK.noFirst = true;
		}

		window.setTimeout
		(
			function()
			{
				document.getElementById("entireLogB").textContent = '';
				document.getElementById("entireLogB").style['max-width']=document.documentElement.clientWidth;//window.innerWidth;
				document.getElementById("entireLogB").style['width']=document.documentElement.clientWidth; //window.innerWidth;

				HTTPUACleaner_LogBView.setView(document.getElementById("entireLogB"), BTable);

				var imgs = document.getElementsByTagName('img');
				for (var img of imgs)
				{
					if (img.id.startsWith('cancel-'))
					{
						img.addEventListener
						("click",
							function()
							{
								UdjtNpJK2wcK.ft(true, img.id.substr(7))
							}
						);
					}
				}
				
				// Здесь эта штука, кажется, не работает. Поэтому прокрутка сама выставляется. Нельзя вызывать в SetTimeout
				// document.documentElement.scrollTop = st;
				// window.scrollY = st;

				document.getElementById("refresh").disabled = false;
				
				//gray.style['z-index'] = '0';
				if (gray && gray.style['display'] != 'none')
				gray.style['display'] = 'none';

				window.scrollBy({top: st});

				if (document.getElementById("todown").checked)
				window.setTimeout
				(
					function()
					{
						window.scrollBy({ top: document.documentElement.clientHeight, behavior: 'smooth'});
					},
					50
				);

				UdjtNpJK2wcK.ft(false);
			},
			50
		);
	}
);

(
function()
{
	let f = function(hlenabled, cancelId)
		{
			self.port.emit
			(
				'showHLog',
				{
					errors: 	document.getElementById("errors")	.checked,
					notGet: 	document.getElementById("notget")	.checked,
					cached: 	document.getElementById("cached")	.checked,
					notcached: 	document.getElementById("notcached").checked,
					validated: 	document.getElementById("validated").checked,
					TLS: 		document.getElementById("TLS")		.checked,
					notTLS: 	document.getElementById("notTLS")	.checked,
					qTLS: 		document.getElementById("qTLS")     .checked,
					inverturl:	document.getElementById("inverturl").checked,
					invertn: 	document.getElementById("invertn")	.checked,
					inverth: 	document.getElementById("inverth")	.checked,
					invertf: 	document.getElementById("invertf")  .checked,
					completed: 	document.getElementById("completed").checked,
					notCompleted:document.getElementById("notCompleted").checked,
					hlenabled:	hlenabled,
					shortLog:	document.getElementById("shortLog").checked,
					headern: 	document.getElementById("headern")  .value,
					headerf: 	document.getElementById("headerf")  .value,
					urlf:		document.getElementById("urlf")     .value,
					fromContent: true, 
					RESET:		false,
					cancelId:	cancelId
				}
			);
		};

	document.getElementById("refresh").addEventListener("click", f);
	document.getElementById("errors") .addEventListener("click", f);
	document.getElementById("notget") .addEventListener("click", f);

	document.getElementById("cached")   .addEventListener("click", f);
	document.getElementById("notcached").addEventListener("click", f);
	document.getElementById("validated").addEventListener("click", f);

	document.getElementById("TLS")   .addEventListener("click", f);
	document.getElementById("notTLS").addEventListener("click", f);
	document.getElementById("qTLS")  .addEventListener("click", f);
	
	document.getElementById("inverturl").addEventListener("click", f);
	document.getElementById("invertn")  .addEventListener("click", f);
	document.getElementById("inverth")  .addEventListener("click", f);
	document.getElementById("invertf")  .addEventListener("click", f);
	
	document.getElementById("completed")    .addEventListener("click", f);
	document.getElementById("notCompleted") .addEventListener("click", f);
	document.getElementById("shortLog")		.addEventListener("click", f);

	document.getElementById("headern")  .addEventListener("change", f);
	document.getElementById("headerf")  .addEventListener("change", f);
	document.getElementById("urlf")     .addEventListener("change", f);
/*
	document.getElementById("headern")  .addEventListener("blur", f);
	document.getElementById("headerf")  .addEventListener("blur", f);
	document.getElementById("urlf")     .addEventListener("blur", f);
*/

	// Запрос на первую перезагрузку, иначе там не получается отображать данные
	f();

	document.getElementById("entireLogB").style['max-width']=document.documentElement.clientWidth; //window.innerWidth;
	document.getElementById("entireLogB").style['width']=document.documentElement.clientWidth; //window.innerWidth;
	
	document.getElementById("reset").addEventListener
	(
		"click", 
		function()
		{
			self.port.emit
			(
				'showHLog',
				{
					RESET: true
				}
			);
		}
	);

	UdjtNpJK2wcK.ft = function(e, cancelId)
	{
		if (e !== true && UdjtNpJK2wcK.timeout || cancelId)
			window.clearTimeout(UdjtNpJK2wcK.timeout);
		UdjtNpJK2wcK.timeout = 0;

		if (!document.getElementById("autoupdate").value)
			return;

		var num = Number(document.getElementById("autoupdate").value);

		if (!Number.isNaN(num) && num > 0)
		{
			if (e === true && document.getElementById("shortLog").checked)
			{
				if (cancelId)
				{
					/*var gray = document.getElementById("grayScreen");
					gray.style['display'] = 'block';*/
				}

				f(undefined, cancelId);
				// UdjtNpJK2wcK.ft(false) - вызывается при обновлении
			}
			else
			UdjtNpJK2wcK.timeout = window.setTimeout
			(
				function()
				{
					UdjtNpJK2wcK.ft(true);
				},
				num * 1000
			);
		}
	};

	document.getElementById("autoupdate").addEventListener
	(
		"change", 
		UdjtNpJK2wcK.ft
	);

	document.getElementById("hlenabled").addEventListener
	(
		"click",
		function()
		{
			f(document.getElementById("hlenabled").checked);
		}
	);
}
)();
