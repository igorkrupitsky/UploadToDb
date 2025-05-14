var oUploadedFiles = [];

function OnLoad() {

    if (_("tbServer"))_("btnDelete").style.display = "";

    if (_("file1").addEventListener) _("file1").addEventListener_
                ("change", FileSelectHandler, false);

    var xhr = new XMLHttpRequest();
    if (xhr.upload) {
        
        var filedrag = _("divDropHere");
        if (filedrag){
            filedrag.addEventListener("dragover", FileDragHover, false);
            filedrag.addEventListener("dragleave", FileDragHover, false);
            filedrag.addEventListener("drop", FileSelectHandler, false);
            filedrag.style.display = "block";
        }

        _("btnUpload").style.display = "none";
    }
}

function FileDragHover(e) {
    e.stopPropagation();
    e.preventDefault();
    e.target.className = (e.type=="dragover")?"hover":"";
}

function FileSelectHandler(e) {
    FileDragHover(e);

    var oFiles = e.target.files || e.dataTransfer.files;
    if (oFiles.length==0) return;

    var sHtml = "<table id='tbClient' class='StatusTable' 
    border=1 cellspacing=0 cellpadding=3><tr>"
        + "<th><a href='?sort=FileName'>File name</a></th>"
        + "<th><a href='?sort=FileSize'>Size</a></th>"
        + "<th><a href='?sort=DateCreated'>Date Modified</a></th>"
        + "<th><label><input type=checkbox name=chkDeleteAll 
          onclick='DeleteAll(this)'>Delete</label></th></tr>";
    for (var i=0; i<oFiles.length; i++){
        sHtml += GetRowHtml(oFiles[i].name, oFiles[i].size, i + "", "");
    }
    
    for (var i=0; i<oUploadedFiles.length; i++){
        sHtml += GetRowHtml(oUploadedFiles[i].name, oUploadedFiles[i].size, "", 
                 oUploadedFiles[i].fileId);
    }
    
    var sServerHtml = "";
    if (_("tbServer")){
        _("trHeader").style.display = "none";
        _("tbServer").style.display = "none";
        sServerHtml = _("tbServer").innerHTML;
    }
    
    _("divStatus").innerHTML = sHtml + sServerHtml + "</table>";
    
    for (var i=0; i<oFiles.length; i++){
        UploadFile(oFiles[i],i);
    }
}

function GetRowHtml(sName, iSize, i, iFileId) {
    var sHref = "";
    if (iFileId != "") sHref = " href='Download.aspx?id=" + iFileId + "' ";

    var s = "<tr><td><a id=fileLink" + i + " target='_blank'" + 
    sHref + ">" + sName + "</a></td>"
              + "<td>" + (iSize/1024).formatNumber(0,',','.') + " KB</td>"
        
        if (i==""){
            s += "<td><div class='progressBar progressSuccess'>&nbsp;</div></td>";
        }else{
            s += "<td id=progressBar"+i+"></td>";
        }

    return s + "<td><input type=checkbox name=chkDelete value=\"" + 
    sName + "\"></td></tr>";
}

function UploadFile(file,i) {
    var xhr = new XMLHttpRequest();
    if (xhr.upload) {
        var progress = _("progressBar"+i).appendChild(document.createElement("div"));
        progress.className = "progressBar";
        progress.innerHTML = "&nbsp;";

        // progress bar
        xhr.upload.addEventListener("progress", function(e) {
            var pc = parseInt(100 - (e.loaded / e.total * 100));
            progress.style.backgroundPosition = pc + "% 0";
        }, false);

        // file received/failed
        xhr.onreadystatechange = function (e) {
            if (xhr.readyState == 4) {
                progress.className = "progressBar " + 
                   (xhr.status == 200 ? "progressSuccess" : "progressFailed");
                if (xhr.status == 200) {

                    if (xhr.responseText == "") {
                        oUploadedFiles.push({ fileId: 0, name: file.name, size: file.size });
                        alert("ccc");
                    } else {
                        eval(xhr.responseText);
                        var iFileId = oUploadedFiles[oUploadedFiles.length - 1].fileId;
                        _("fileLink" + i).href = "Download.aspx?id=" + iFileId;
                    }

                    _("btnDelete").style.display = ""
                } else {
                    _("divError").innerHTML = xhr.responseText;
                }
            }
        };

        var oFormData = new FormData();
        oFormData.append("myfile"+i, file);
        xhr.open("POST", _("form1").action, true);
        xhr.send(oFormData);
    }
}

function DeleteAll(o){
    var oBoxes = document.getElementsByTagName("input");
    for (var i=1; i<oBoxes.length; i++){
        oBoxes[i].checked = o.checked;
    }
}

function _(id) {
    return document.getElementById(id);
}

Number.prototype.formatNumber = function(decPlaces, thouSeparator, decSeparator) {
    var n = this,
        decPlaces = isNaN(decPlaces = Math.abs(decPlaces)) ? 2 : decPlaces,
        decSeparator = decSeparator == undefined ? "." : decSeparator,
        thouSeparator = thouSeparator == undefined ? "," : thouSeparator,
        sign = n < 0 ? "-" : "",
        i = parseInt(n = Math.abs(+n || 0).toFixed(decPlaces)) + "",
        j = (j = i.length) > 3 ? j % 3 : 0;
    return sign + (j ? i.substr(0, j) + thouSeparator : "") + 
           i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thouSeparator) + 
           (decPlaces ? decSeparator + Math.abs(n - i).toFixed(decPlaces).slice(2) : "");
};
