HTTPUACleaner.mutex = function(maxParallelsCount)
{
	this.maxParallelsCount = maxParallelsCount;
	this.name = '';

	this.released = true;
	this.queue    = [];
	this.log      = {lastEnter: null, lastReleased: null};
};

// Если true - вошли в мьютекс, иначе callback ставится на выполнение, а защищённая секция кода не должна быть выполенна
// Если callbackExecute, то callback вызовется в любом случае. callback сам должен вызвать освобождение мьютекса
// Если !callbackExecute, то callback вызовется только если не удалось войти в мьютекс.
// В таком случае, callback должен не только освободить мьютекс, но и получить его
HTTPUACleaner.mutex.prototype.enter = function(callback, t, callbackExecute)
{
	if (this.released === true)
	{
		if (!this.inRelease && this.queue.length > 0)
		{
			let errMsg = 'HUAC ERROR: mutex.enter ' + this.name +  ' is released but queue not empty';
			HTTPUACleaner.logMessage(errMsg, true);
			HTTPUACleaner.logCallers();
			HTTPUACleaner.logObject(this);
		}

		this.released = false;
		this.log.lastEnter = HTTPUACleaner.logCallers(true);

		if (callbackExecute)
		{
			if (!callback)
			{
				HTTPUACleaner.logMessage('HUAC ERROR: mutex.enter ' + this.name +  ': callback not setted (callbackExecute)', true);
				HTTPUACleaner.logCallers();
				HTTPUACleaner.logObject(this);

				return true;
			}

			if (t)
				callback.bind(t)();
			else
				callback();
		}

		return true;
	}

	if (!callback)
	{
		HTTPUACleaner.logMessage('HUAC ERROR: mutex.enter ' + this.name +  ': callback not setted', true);
		HTTPUACleaner.logCallers();
		HTTPUACleaner.logObject(this);

		return false;
	}

	var now = Date.now();
	this.queue.push([callback, t, now, HTTPUACleaner.logCallers(true), callbackExecute]);

	if (this.queue.length > this.maxParallelsCount)
	{
		let errMsgP = 'HUAC ERROR: mutex.enter ' + this.name +  ' very most parallel: ' + this.queue.length + '. Please, send message to developer ( prg@vs8.ru )';

		HTTPUACleaner.logMessage(errMsgP, true);
		HTTPUACleaner.logCallers();
		HTTPUACleaner.logObject(this);
		HTTPUACleaner.logObject(t);
	}

	if (now - this.queue[0][2] > 20 * 1000)
	{
		let errMsgD = 'HUAC ERROR: mutex.enter ' + this.name +  ' long time lock: ' + this.queue.length + '. Please, send message to developer ( prg@vs8.ru )';

		HTTPUACleaner.logMessage(errMsgD, true);
		if (this.log.lastEnter)
			HTTPUACleaner.logObject("lastEnter:\r\n" + this.log.lastEnter);
		HTTPUACleaner.logCallers();
		HTTPUACleaner.logObject(this);
		HTTPUACleaner.logObject(t);
	}

	return false;
};

HTTPUACleaner.mutex.prototype.release = function()
{
	if(this.released === true)
	{
		HTTPUACleaner.logMessage('HUAC ERROR: mutex.release ' + this.name +  ' called with the released mutex', true);
		HTTPUACleaner.logCallers();
		HTTPUACleaner.logObject(this);
	}

	this.released = true;
	this.log.lastReleased = "Enter:\r\n" + this.log.lastEnter + "\r\nRelease:\r\n" + HTTPUACleaner.logCallers(true) + "\r\n";
	this.log.lastEnter = null;

	if (this.queue.length > 0)
	{
		// Предполагаетсяе, что мьютекс сейчас свободен и callback его получит
		var callback = this.queue.shift();
		try
		{
			this.inRelease = true;
			// Если мьютекс используется как обёртка над лексическим замыканием,
			// то он предполагает автоматическое получение мьютекса при вызове
			if (callback[4])
				this.released = false;

			this.log.lastEnter = callback[3];
			var fn = callback[0];
			if (callback[1])
				fn.bind(callback[1])();
			else
				fn();
		}
		catch (e)
		{
			HTTPUACleaner.logMessage('HUAC ERROR: mutex.release ' + this.name +  ' the release queue error', true);
			HTTPUACleaner.logCallers();
			HTTPUACleaner.logObject(this);
			HTTPUACleaner.logObject(callback);
		}

		return false;
	}

	this.inRelease = false;

	return true;
};
