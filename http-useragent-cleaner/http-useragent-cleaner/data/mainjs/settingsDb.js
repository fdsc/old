const {TextDecoder, TextEncoder, OS} = Cu.import("resource://gre/modules/osfile.jsm", {});

HTTPUACleaner.sdbP = function(fileName, callback)
{
	this.fileMutex      = new HTTPUACleaner.mutex(3);
	this.setHttpAllowed = new HTTPUACleaner.mutex(1);
	this.fileMutex.name      = 'fileMutex.sdbP';
	this.setHttpAllowed.name = 'setHttpAllowed';

	this.dir      = OS.Path.join(OS.Constants.Path.profileDir, 'HTTPUACleaner');
	this.fileName = OS.Path.join(this.dir, fileName);
	this.decoder  = new TextDecoder();
	this.encoder  = new TextEncoder();  
	
	var promise = OS.File.makeDir(this.dir, {ignoreExisting: true});

	this.startupError = 1;

	this.setHttpAllowed.enter();

	promise.then
	(
		function()
		{
			if (callback)
				callback(true);
		},
		function()
		{
			if (callback)
				callback(false);
		}
	).catch(console.error);
};

HTTPUACleaner.sdbP.prototype.defaultStr = '{}';

// Не вызывать напрямую, пользоваться SetOptions
HTTPUACleaner.sdbP.prototype.saveSettings = function(settings, callback, sc, callbackMayNotCall)
{
	if (HTTPUACleaner.terminated)
	{
		var warnMsg = 'HUAC warning: save (main settings) cancelled because HUAC terminated';
		console.error(warnMsg);
		HTTPUACleaner.logMessage(warnMsg);
		HTTPUACleaner.logCallers();
		return;
	}

	if (this.startupError === true || HTTPUACleaner.errorLoad || this.startupError === 1)
	{
		var warnMsg = 'HUAC FATAL ERROR: save (main settings) cancelled because HUAC error in startup (' + this.startupError + ', ' + HTTPUACleaner.errorLoad + ')';
		HTTPUACleaner.logMessage(warnMsg, true);
		HTTPUACleaner.logCallers();

		return;
	}

	var cb = function()
	{
		this.saveSettings(settings, callback, sc, callbackMayNotCall);
	};

	if (!this.fileMutex.enter(cb, this))
		return;

	var t = this;

	try
	{
		this.save
		(
			settings,
			function(result)
			{
				try
				{
					if (callback)
						callback(result, t);
				}
				catch(e)
				{
					HTTPUACleaner.logMessage('Load settings failed: callback error');
					HTTPUACleaner.logObject(e, true);
				}

				t.fileMutex.release();
			},
			sc
		);
	}
	catch(e)
	{
		HTTPUACleaner.logMessage('HUAC FATAL ERROR: Save settings failed: call load error', true);
		HTTPUACleaner.logObject(e, true);

		this.fileMutex.release();
	}

	return true;
};

// Функция вызывается в начале работы дополнения, когда другие объекты ещё не заполнены
HTTPUACleaner.sdbP.prototype.loadSettings = function(callback)
{
	var cb = function()
	{
		this.loadSettings(callback);
	};

	if (!this.fileMutex.enter(cb, this))
		return;

	let promise = OS.File.open(this.fileName, {existing: false, read: true, write: true, append: false, truncate: false});

	var t = this;

	var read = function(file)
	{
		let promise = file.read();

		promise.then
		(
			function onSuccess(array)
			{
				try
				{
					file.close();

					var str = false;
					try
					{
						str = t.decoder.decode(array);
						
						if (str.length <= 0)
							str = t.defaultStr;

						str = JSON.parse(str);
						t.settings = str;

						if (t.startupError === 1)
							t.startupError = 2;

						if (callback)
						{
							if (str !== false)
								callback(true, str);
							else
								callback(false);
						}
					}
					catch (e)
					{
						if (callback)
							callback(false);

						HTTPUACleaner.logObject(e, true, true);
						str = false;
					}

					if (HTTPUACleaner['sdk/preferences/service'].get(HTTPUACleaner_Prefix + 'debug.writeSettingFilePathes', false))
					{
						console.error('HUAC opened "side" settings file ' + t.fileName);
					}
				}
				catch(e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}

				t.fileMutex.release();
					
				return true;
			},
			
			function onFailure(reason)
			{
				/*if (reason instanceof OS.File.Error && reason.becauseNoSuchFile)
				{
					
				}*/

				if (t.startupError === 1)
				{
					t.startupError = true;
					HTTPUACleaner.errorLoad = true;

					try	
					{
						HTTPUACleaner.terminated = false;
						HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('startuperror.html'), undefined, undefined, true);
					}
					catch (e)
					{}

					HTTPUACleaner.terminated = true;
				}

				try
				{
					file.close();
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}

				try
				{
					console.error('------------------------------------------------------------------------------------------------------------------');
					console.error('HUAC: Fatal error in load sdb settings');
					console.error(t.fileName);
					console.error(reason);
					
					HTTPUACleaner.logMessage('HUAC: Fatal error in load sdb settings (read)');
					HTTPUACleaner.logObject(t.fileName);
					HTTPUACleaner.logObject(reason);
				}
				catch (e)
				{
					HTTPUACleaner.logMessage('HUAC: Fatal error in load sdb settings (ex; read)');
					HTTPUACleaner.logObject(e, true);
				}

				try
				{
					if (callback)
						callback(false);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}
				
				t.fileMutex.release();
			}
		).catch
		(
			function(e)
			{
				if (t.startupError === 1)
				{
					t.startupError = true;
					HTTPUACleaner.errorLoad = true;

					try	
					{
						HTTPUACleaner.terminated = false;
						HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('startuperror.html'), undefined, undefined, true);
					}
					catch (e)
					{}

					HTTPUACleaner.terminated = true;
				}

				try
				{
					file.close();
				}
				catch(e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}

				try
				{
					if (callback)
						callback(false);
				}
				catch (e)
				{
					HTTPUACleaner.logObject(e, true, true);
				}

				HTTPUACleaner.logMessage('HUAC: Fatal error in load sdb settings (catch; read)');
				HTTPUACleaner.logObject(e, true);

				t.fileMutex.release();
			}
		);
	};
	
	promise.then
	(
		function onSuccess(file)
		{
			read(file);
			return true;
		},
		
		function onFailure(reason)
		{
			/*if (reason instanceof OS.File.Error && reason.becauseNoSuchFile)
			{
				
			}*/
			
			try
			{
				if (t.startupError === 1)
				{
					t.startupError = true;
					HTTPUACleaner.errorLoad = true;

					try	
					{
						HTTPUACleaner.terminated = false;
						HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('startuperror.html'), undefined, undefined, true);
					}
					catch (e)
					{}

					HTTPUACleaner.terminated = true;
				}
			}
			catch(e)
			{
				console.error(e);
			}

			try
			{
				console.error('------------------------------------------------------------------------------------------------------------------');
				console.error('HUAC: Fatal error in load sdb settings');
				console.error(t.fileName);
				console.error(reason);
				
				HTTPUACleaner.logMessage('HUAC: Fatal error in load sdb settings (open)');
				HTTPUACleaner.logObject(t.fileName);
				HTTPUACleaner.logObject(reason);
			}
			catch (e)
			{
				HTTPUACleaner.logMessage('HUAC: Fatal error in load sdb settings (ex; open)');
				HTTPUACleaner.logObject(e, true);
			}

			try
			{
				if (callback)
					callback(false);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true, true);
			}

			t.fileMutex.release();
		}
	).catch
	(
		function(e)
		{
			try
			{
				if (t.startupError === 1)
				{
					t.startupError = true;
					HTTPUACleaner.errorLoad = true;

					try	
					{
						HTTPUACleaner.terminated = false;
						HTTPUACleaner.tabOpen(HTTPUACleaner['sdk/self'].data.url('startuperror.html'), undefined, undefined, true);
					}
					catch (e)
					{}

					HTTPUACleaner.terminated = true;
				}
			}
			catch(e)
			{
				console.error(e);
			}


			try
			{
				HTTPUACleaner.logMessage('HUAC: Fatal error in load sdb settings (catch; open)');
				HTTPUACleaner.logObject(e, true);
			}
			catch (e)
			{
				console.error(e);
			}

			try
			{
				if (callback)
					callback(false);
			}
			catch (e)
			{
				HTTPUACleaner.logObject(e, true, true);
			}

			t.fileMutex.release();
		}
	);
};

HTTPUACleaner.sdbP.prototype.save = function(toSave, callback, sc)
{
	// var promise = OS.File.open(this.fileName, {existing: false, read: false, write: true, append: false, truncate: true});
	var t = this;

	var write = function(retry)
	{
		var toSaveStr = JSON.stringify(toSave);
		let now = Date.now();

		// --------------------
		if (retry === 0)
		try
		{
			if (!toSave.lastSave || now - toSave.lastSave.time > 1000*3600*24*30 || toSaveStr.length/toSave.lastSave.size > 1.189 || Math.abs(toSaveStr.length - toSave.lastSave.size) > 16384)
			{
				toSave.lastSave = {time: now, size: toSaveStr.length + 1};// Чтобы выше не было деления на ноль
				sc.lastSave     = toSave.lastSave;
				toSaveStr = JSON.stringify(toSave);
				let encoded = t.encoder.encode(toSaveStr);

				var copyFileName = t.fileName + '.' + now + '.copy';
				let copyPromise = OS.File.writeAtomic(copyFileName, encoded, {tmpPath: copyFileName + '.tmp', noOverwrite: false, flush: true});
				copyPromise.then
				(
					function onSuccess()
					{},
					function onFailure(reason)
					{
						console.error(reason);

						HTTPUACleaner.logMessage('save error (HTTPUACleaner.sdbP.prototype.save (copy) g009bkS6KJlm)');
						HTTPUACleaner.logObject(reason);
					}
				).catch
				(
					function(e)
					{
						HTTPUACleaner.logMessage('save error (HTTPUACleaner.sdbP.prototype.save (copy) AuQXqA9bR7Vk)');
						HTTPUACleaner.logObject(e, true);
					}
				);
			}
		}
		catch (e)
		{
			HTTPUACleaner.logObject(e, true);
		}

		// --------------------

		try
		{
			toSaveStr = JSON.stringify(toSave);
			// Похоже, что encoder не может использовать одно и то же преобразование дважды
			let encoded = t.encoder.encode(toSaveStr);

			let promise = OS.File.writeAtomic(t.fileName, encoded, {tmpPath: t.fileName + '.copy', noOverwrite: false});
			promise.then
			(
				function onSuccess()
				{
					if (callback)
					try
					{
						callback(true);
					}
					catch(e)
					{
						HTTPUACleaner.logObject(e, true, true);
					}
				},
				function onFailure(reason)
				{
					console.error(reason);

					HTTPUACleaner.logMessage('save error (HTTPUACleaner.sdbP.prototype.save fLRpoi4gnrT3)', true);
					HTTPUACleaner.logObject(reason);

					if (retry < 2)
					{
						write(retry + 1);
						return;
					}

					if (callback)
					try
					{
						callback(false);
					}
					catch(e)
					{
						HTTPUACleaner.logObject(e, true, true);
					}
				}
			).catch
			(
				function(e)
				{
					HTTPUACleaner.logMessage('save error (HTTPUACleaner.sdbP.prototype.save fYFQZ62tKXcp)');
					HTTPUACleaner.logObject(e, true);
					
					if (retry < 2)
					{
						write(retry + 1);
						return;
					}
					
					if (callback)
					try
					{
						callback(false);
					}
					catch(e)
					{
						HTTPUACleaner.logObject(e, true, true);
					}
				}
			);
		}
		catch (e)
		{
			HTTPUACleaner.logObject(e, true);
		}
	};

	write(0);
};
