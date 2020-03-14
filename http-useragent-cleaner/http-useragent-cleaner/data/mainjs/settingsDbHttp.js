HTTPUACleaner.sdbP.prototype.startUpContinue = function()
{
	if (this.startUpContinued)
		return;

	try
	{
		if (HTTPUACleaner.httpOptionsInitialized())
		{
			this.startUpContinueInternal();
			return;
		}

		let t = this;
		HTTPUACleaner.timers.setTimeout
		(
			function()
			{
				t.startUpContinue();
			},
			500
		)
	}
	catch (e)
	{
		HTTPUACleaner.logMessage('startup error (startUpContinue)');
		HTTPUACleaner.logObject(e);

		throw e;
	}
};

HTTPUACleaner.sdbP.prototype.startUpContinueInternal = function()
{
	if (this.startUpContinued)
		return;

	try
	{
		if (!HTTPUACleaner.storage.ciphersOptions)
			HTTPUACleaner.getCiphers();
		else
			HTTPUACleaner.RestoreAllCiphersState();

		// �����, �.�. RestoreAllCiphersState ����� �� ���������� ������, ��� ����
		HTTPUACleaner.addPrefsObserver(true);

		this.startUpContinued = true;

		HTTPUACleaner.observer.observe(undefined);
	}
	catch (e)
	{
		HTTPUACleaner.logMessage('startup error (startUpContinueInternal)');
		HTTPUACleaner.logObject(e);

		throw e;
	}
};

HTTPUACleaner.sdbP.prototype.initHttpOptions = function()
{
	if (this.httpInitialized && this.currentSettings)
		return this.currentSettings.http;

	if (!this.currentSettings)
		return null;

	// ���� �� �����������������, �� ���� ������ �� ������� ��������� storage
	if (!this.currentSettings.http)
	{
		this.currentSettings.http = JSON.parse(JSON.stringify(HTTPUACleaner.storage));
		if (!this.currentSettings.http)
			this.currentSettings.http = {};

		var t = this;
		this.setHttpAllowed.enter
		(
			function()
			{
				this.setHTTP
				(
					function()
					{
						t.setHttpAllowed.release();

						if (HTTPUACleaner.simplestorage && HTTPUACleaner.simplestorage.storage)
						{
							delete HTTPUACleaner.simplestorage.storage.enabledOptions;
							delete HTTPUACleaner.simplestorage.storage.ciphersOptions;
						}
					}
				);
			},
			this,
			true
		);
	}
	else
		// �� ������ ������ ���������� simplestorage, ����� � ��������� ��� �� ��� �� ��������, ���� ������ ��������,
		// �� ��� ���� http ��������� ��� ����
		{
			if (HTTPUACleaner.simplestorage && HTTPUACleaner.simplestorage.storage)
			{
				delete HTTPUACleaner.simplestorage.storage.enabledOptions;
				delete HTTPUACleaner.simplestorage.storage.ciphersOptions;
			}
		}

	// ������������, setHTTP ����� �������� ���������� initHttpOptions � �� ��� ����� ��������� ��� ����
	if (this.httpInitialized)
		return this.currentSettings.http;

	// �������������� ����� ������ ��� ������ ������ ���������, ��� HTTPUACleaner.storage = httpOptions;
	HTTPUACleaner.initStorageOptions(this.currentSettings.http);
	this.httpInitialized = true;

	return this.currentSettings.http;
};

// HTTPUACleaner.sdb.setHTTP();
HTTPUACleaner.sdbP.prototype.setHTTP = function(callback)
{
	if (!this.currentSettings)
		return false;

	this.setOptions(callback);

	return true;
};
