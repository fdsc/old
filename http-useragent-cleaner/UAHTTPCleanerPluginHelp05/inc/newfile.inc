<?php

function generateNew($scriptFileName, $oldFileToDelete)
{
	header("Cache-Control: max-age=0, must-revalidate, proxy-revalidate, no-cache, no-store, private", true);

	$scriptFileNameSlash = strrpos($scriptFileName, '/');
	$workpiece = substr($scriptFileName, $scriptFileNameSlash + 1);
	$scriptFileNameSlash = strrpos($workpiece, '.htm');
	$workpiece = substr($workpiece, 0, $scriptFileNameSlash);
	$workpieceFull = $workpiece . '-.htm';

	if (file_exists($workpieceFull))
	{
		$f = fopen($scriptFileName, 'r');
		flock($f, LOCK_EX);
		$time = time();
		$success = false;
		if (rename($workpieceFull, $workpiece . '-' . $time . '-perm.htm'))
		{
			$oldContent = file_get_contents($scriptFileName);
			$newContent = preg_replace('/' . $workpiece . '-[0-9]+-perm.htm/', $workpiece . '-' . $time . '-perm.htm', $oldContent);
			file_put_contents($scriptFileName, $newContent);

			echo 'generated ' . $time . '<br>';
			$success = true;

			if ($oldFileToDelete)
			if (!unlink($oldFileToDelete))
				echo 'file ' . $oldFileToDelete . ' impossible to delete';
		}

		flock($f, LOCK_UN);
		fclose($f);

		if ($success)
			exit(0);
		else
			echo 'error<br>';
	}
	else
	{
		echo 'file ' . $workpieceFull . ' not exists<br>';
		exit(0);
	}

	exit(0);
};

?>