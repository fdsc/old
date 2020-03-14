<?php

function isTrivialViolation($report)
{
	if (  $report["blocked-uri"] == 'inline' && !isset($report['script-sample'])  )
		return true;

	$scripts = 
		array
		(
			array("@media print {#UNIQUE_ID-ghostery {displ...", "self"),
			//array("call to eval() or related function blocked by CSP", "self"),
			array("@media print {#ghostery-purple-box {disp...", "self")
		);

	for ($i = 0; $i < count($scripts); $i++)
	{
		$instance = $scripts[$i];
		if ($instance[1] == $report["blocked-uri"] && $instance[0] == $report['script-sample'])
			return true;
	}
	
	$viewSource = 'view-source:http';
	if (  substr( $report["blocked-uri"], 0, strlen($viewSource) ) == $viewSource  )
		return true;

	return false;
};

?>