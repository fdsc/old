var HTTPUACleanerDialogObject =
{
	SetNoDialog: function()
	{
		self.port.emit("SetNoDialog");
		HTTPUACleanerDialogObject.Close();
	},

	Close: function()
	{
		self.port.emit("CloseDialog");
	}
};

var CloseElement 	= document.getElementById("close");
var NoDialogElement = document.getElementById("nodialog");

CloseElement	.onclick = HTTPUACleanerDialogObject.Close;
if (NoDialogElement)
	NoDialogElement	.onclick = HTTPUACleanerDialogObject.SetNoDialog;


self.port.on
(
	'initCipherSettingsDialog',
	function(opt)
	{
		CloseElement.value = opt.strings.OK;
		if (NoDialogElement)
			NoDialogElement.value = opt.strings.NoMoreDialogs;
	}
);
