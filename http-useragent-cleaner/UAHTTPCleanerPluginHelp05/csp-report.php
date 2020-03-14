<?php

include 'inc/trivial.inc';

// $a = $_POST;
// php://stdin
$a = file_get_contents('php://input', false, NULL, 0, 4096);
$c = $a;
$report = json_decode($a, true);

function testPrint($fContent)
{
	$f = fopen('./stat/test', 'a');
	flock($f, LOCK_EX);

	fwrite($f, date('Y-m-d H:i:s') . "\r\n" . $fContent . " \r\n\r\n");

	flock($f, LOCK_UN);
	fclose($f);
};

function toBase64_64($str)
{
	$str = str_replace('/', '-', base64_encode($str)); // bin2hex
	$str = substr($str, 0, 64);
	return $str;
};

function getHelpData()
{
	$str = '';
	if (isset($_SERVER['REMOTE_ADDR']))
		$str = $_SERVER['REMOTE_ADDR'];

	if (isset($_SERVER['HTTP_USER_AGENT']))
		$str .= '        ' . $_SERVER['HTTP_USER_AGENT'];

	return $str;
};

function csp_error($c, $reason)
{
	$fName = './stat/_error-csp-' . date('Y-m-d');
	$fSize = file_exists($fName) ? filesize($fName) : 0;
	if ($fSize < 8192)
	{
		$f = fopen($fName, 'a');
		if (!flock($f, LOCK_EX))
			echo 'Error with lock acquiring (csp_error)';

		fwrite($f,  date('Y-m-d H:i:s') . "\r\n" . getHelpData() . "\r\n" . $reason . "\r\n" . $c . " \r\n\r\n");
		flock($f, LOCK_UN);
		fclose($f);
	}
};

// Если данные ошибочны
if (!$report || !$report['csp-report'])
{
	csp_error($c, 'csp-report empty');
	exit(1);
}

$report = $report['csp-report'];

if ($report && !isTrivialViolation($report))
{
	$a = json_encode($report, 128 | 64 /*JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES | JSON_PARTIAL_OUTPUT_ON_ERROR*/);
	$a = preg_replace('/^([\t\s]*)(.*)\{[\t\s]*$/m', "$1$2\r\n$1{", $a);

	if (strlen($report['script-sample']) > 0)
		$binhexShort = toBase64_64($report['script-sample']); // bin2hex
	else
		$binhexShort = '';

	$emptyReportScript = strlen($binhexShort) < 1;
	
	if ($emptyReportScript)
	{
		$binhexShort = toBase64_64(strlen($report['original-policy']) . $report['blocked-uri'] . '/' . $report['violated-directive']);
	}

	$fName = './stat/report' . date('Y-W') . "-" . $binhexShort;
	$fSize = file_exists($fName) ? filesize($fName) : 0;

	if ($fSize < 8192 || ($emptyReportScript && $fSize < 16384) || ($emptyReportScript && strlen($binhexShort) < 16 && $fSize < 65536))
	{
		$f = fopen($fName, 'a');
		if (!f)
		{
			csp_error('' . $f . "\r\n" . $fName, 'Error during file open');
		}

		if (!flock($f, LOCK_EX))
		{
			echo 'Error with lock acquiring';
			csp_error('' . $f, 'Error with lock acquiring');
		}

		if ($a && strlen($a) > 4)
			fwrite($f,  date('Y-m-d H:i:s') . "\r\n" . getHelpData() . "\r\n" . $a . " \r\n\r\n");
		else
			fwrite($f,  date('Y-m-d H:i:s') . "\r\n" . getHelpData() . " (ERROR)\r\n" . $c . " \r\n\r\n");

		flock($f, LOCK_UN);
		fclose($f);
	}
}

?>