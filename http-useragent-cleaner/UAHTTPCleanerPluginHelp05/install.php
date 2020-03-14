<?php

header('Content-Type: application/rdf+xml');
header('Cache-Control: max-age=0, must-revalidate, proxy-revalidate, no-cache, no-store, private');
$inst = file_get_contents("install.rdf");

echo $inst;


$a = $_SERVER['HTTP_ACCEPT_LANGUAGE'];

$f = fopen('./stat/stat' . date('Y-m-d') . '.log', 'a');
fwrite($f,  date('Y-m-d H:i:s') . "\r\n");
fclose($f);

?>
