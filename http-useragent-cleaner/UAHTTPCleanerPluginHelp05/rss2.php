<?php
header('Content-Type: application/rss+xml');
header('Cache-Control: max-age=0, must-revalidate, proxy-revalidate, no-cache, no-store, private');

echo '<?xml version="1.0" encoding="utf-8"?>';

$a = $_SERVER['HTTP_ACCEPT_LANGUAGE'];
$i = strpos($a, 'RU');
 ?>
 
<rss version="2.0"  xmlns:atom="http://www.w3.org/2005/Atom">
<channel>
<atom:link href="http://huac.8vs.ru/rss.php" rel="self" type="application/rss+xml" />
<title>huac.8vs.ru news</title>
<link>http://huac.8vs.ru/</link>
<lastBuildDate>Thu, 17 Dec 2015 09:08:51 GMT</lastBuildDate>

<?php
if ($i === FALSE)
{
 ?>

<description>huac.8vs.ru news</description>

<item>
	<title>In FireFox 43 you must set the xpinstall.signatures.required setting to false</title>
	<description>In FireFox 43 you must set the xpinstall.signatures.required setting to false for provide the extension work. See about:config tab in FireFox.
	</description>

<?php
}
else
{
?>

<description>Новости huac.8vs.ru</description>

<item>
	<title>В FireFox 43 необходимо установить настройку xpinstall.signatures.required в false</title>
	<description>В FireFox 43 необходимо установить настройку xpinstall.signatures.required в false, чтобы дополнение работало. Это можно сделать, если открыть в FireFox вкладку about:config.
	</description>

<?php
}
?>
	<category>update</category>
	<link>http://huac.8vs.ru/indexo2.html#iCookies</link>
	<pubDate>Thu, 17 Dec 2015 09:08:51 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201512170</guid>
</item>


</channel>
</rss>

<?php

$f = fopen('./stat/statrss' . date('Y-m-d') . '.log', 'a');
fwrite($f, date('H:i:s') . ' ' . $a . "\r\n");
fclose($f);

?>
