// © Виноградов Сергей Васильевич, 2007var CurrentPage = new _Page('.');

window.onload = function()
{
    CurrentPage.onLoad();
    if (window.winOnLoad)
        window.winOnLoad();
}

window.onunload = function()
{
    CurrentPage.onUnload();
    if (window.winOnUnload)
        window.winOnUnload();
}

function _Page(sRoot)
{
    this.siteRoot = sRoot;
    
    this.onLoad   = function()
    {
        if (!this.downgrade)
        {
            this._CElements = new _Collapsible();
        }
    }
    
    this.onUnload = function()
    {
        if (this._CElement && this._CElement.onUnload) this._CElement.onUnload();
    }

    this.downgrade = true;
    if (document.getElementById || document.all)
    {
        this.downgrade = false;
    }
}

function _GetElementById(_element)
{
    if ( typeof(_element) != 'string' ) return null;

    if ( document.getElementById ) _element = document.getElementById(_element);
    else if(document.all) _element = document.all[_element];
    else _element = null;
    
    return _element;
}

function _GetElementsByTagName(_tag, _parent)
{
    var list = null;
    
    if (_tag == null || _parent == null)
    return list;

    if (_parent.getElementsByTagName) list = _parent.getElementsByTagName(_tag);
    else if (_parent.all) list = _parent.all.tags(_tag);
    return list;
}

function _PrevSibling(_element, _tag, _className)
{
    if (_element == null)
        return null;
 
    var s = _element.previousSibling;    
    if (_tag)
        while(s && (s.nodeName != _tag || s.className == "" || s.className != _className)  )
        {
            s=s.previousSibling;
        }
    else
        while(s && (s.nodeName != _tag || s.className == "" || s.className != _className)  )
        {
            s=s.previousSibling;
        }
        
    return s;
}

function _NextSibling(_element, _tag, _className)
{
    if (_element == null)
        return null;
 
    var s = _element.nextSibling;    
    if (_tag)
        while(s && (s.nodeName != _tag || s.className == "" || s.className != _className)  )
        {
            s=s.nextSibling;
        }
    else
        while(s && (s.nodeName != _tag || s.className == "" || s.className != _className)  )
        {
            s=s.nextSibling;
        }
        
    return s;
}

function _Collapsible()
{
    var trg, tClose, aTgt = _GetElementsByTagName('DIV', document);
    
    var UN   = _GetElementById("UsageNote");
    if (UN)  UN.style.display = "block";
    var UN_  = _GetElementById("UsageNote_");
    if (UN_) UN_.style.display = "none";
  
    for (i = 0; i < aTgt.length; i++)
    {
        var EC = aTgt[i].className == "ExpanderC";
        var EO = aTgt[i].className == "ExpanderO";
        if (!EC && !EO) continue;

        trg = _PrevSibling(aTgt[i], "IMG", "Expander");
        tClose = _NextSibling(aTgt[i], "IMG", "Closer");
        if (trg)
        {
            aTgt[i]._TrgPtr = trg;
            
            if (EO)
            {
                var im = _GetElementById("img8m");
                trg.src = im.src;
                
                aTgt[i].style.display = 'block';
                trg.alt = "-";
            }
            else
            {
                var im = _GetElementById("img8p");
                trg.src = im.src;
            
                aTgt[i].style.display = 'none';
                trg.alt = "+";
            }
            
            trg.style.display = 'inline';
            trg._Closer = tClose;
            
            if (tClose)
            {
                if (EO)
                     tClose.style.display = 'block';
                else tClose.style.display = 'none';
                
                tClose.alt = "--";

                var im = _GetElementById("img8c");
                tClose.src = im.src;
                    
                tClose._trg = trg;
                tClose.onclick = CloserClick;
            }
                
            trg.onclick = trg_onClick;
            trg._TgtPtr = aTgt[i];
        }
    }
  
    function trg_onClick()
    {
        var tgt = this._TgtPtr.style;
        if (tgt.display == 'none')
        {
            var im = _GetElementById("img8m");
            this.src = im.src;
            
            tgt.display = 'block';
		    this.alt = "-";
        }
        else
        {
            var im = _GetElementById("img8p");
            this.src = im.src;
            
            tgt.display = 'none';
		    this.alt = "+";
        }
        
        if (this._Closer)
		    this._Closer.style.display = tgt.display;
    }
    
    function CloserClick()
    {
        this._trg.onclick();
    }
    
    this.onUnload = function()
    {
        for (i = 0; i < aTgt.length; ++i)
        {
            trg = aTgt[i]._TrgPtr;
            if (trg)
            {
                if (trg._TgtPtr)
                {
                    trg._TgtPtr._TrgPtr = null;
                    trg._TgtPtr = null;
                }
                if (trg._Closer)
                {
                    trg._Closer._trg = null;
                    trg._Closer = null;
                }
                trg.onclick = null;
            }
        }
    };
}
