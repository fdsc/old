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
<link>http://huac.8vs.ru/</link>
<?php
if ($i === FALSE)
{
 ?>
 <title>huac.8vs.ru news</title>
<description>huac.8vs.ru news</description>
<?php
}
else
{
?>
<title>Новости huac.8vs.ru</title>
<description>Новости huac.8vs.ru</description>
<?php
}
?>

<lastBuildDate>Sat, 06 Aug 2016 20:10:46 GMT</lastBuildDate>



<item>
	<title>2.0.4-b08</title>
	<description>Добавлены условия правил fta и cta. Запросы по схеме resource: теперь проходят через фильтрацию, если направляются со страницы. Скорректирована оценка стойкости TLS. Добавлена настройка truncLenght.url. Добавлена дополнительная информация в лог. Добавлена возможность перемещения правил. Добавлена возможность разблокировки OCSP при инициализации дополнения. Подробнее см. на странице новостей.
	</description>
	<category>update</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Sat, 06 Aug 2016 20:10:46 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201608060</guid>
</item>


<item>
<?php
if ($i === FALSE)
{
?>
	<title>FireFox 48</title>
	<description>In FireFox 48, the not signed addons does not executed. You need to download signed version of the Extension http://huac.8vs.ru/http_useragent_cleaner_o-2.0.1-b05-fx.xpi
	</description>

<?php
}
else
{
?>
	<title>FireFox 48</title>
	<description>В FireFox 48 неподписанные дополнения не запускаются. Пользователям неподписанной версии дополнения необходимо скачать подписанную версию. http://huac.8vs.ru/http_useragent_cleaner_o-2.0.1-b05-fx.xpi
	</description>

<?php
}
?>
	<category>update</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Wed, 03 Aug 2016 18:51:11 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201608030</guid>
</item>



</channel>
</rss>

<?php

$f = fopen('./stat/statrss' . date('Y-m-d') . '.log', 'a');
fwrite($f, date('H:i:s') . ' ' . $a . "\r\n");
fclose($f);

?>
