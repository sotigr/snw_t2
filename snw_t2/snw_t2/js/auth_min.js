

//hashtable extensions
function HashTable(obj) {
    this.length = 0;
    this.items = {};
    for (var p in obj) {
        if (obj.hasOwnProperty(p)) {
            this.items[p] = obj[p];
            this.length++;
        }
    }

    this.setItem = function (key, value) {
        var previous = undefined;
        if (this.hasItem(key)) {
            previous = this.items[key];
        }
        else {
            this.length++;
        }
        this.items[key] = value;
        return previous;
    }

    this.getItem = function (key) {
        return this.hasItem(key) ? this.items[key] : undefined;
    }

    this.hasItem = function (key) {
        return this.items.hasOwnProperty(key);
    }

    this.removeItem = function (key) {
        if (this.hasItem(key)) {
            previous = this.items[key];
            this.length--;
            delete this.items[key];
            return previous;
        }
        else {
            return undefined;
        }
    }

    this.keys = function () {
        var keys = [];
        for (var k in this.items) {
            if (this.hasItem(k)) {
                keys.push(k);
            }
        }
        return keys;
    }

    this.values = function () {
        var values = [];
        for (var k in this.items) {
            if (this.hasItem(k)) {
                values.push(this.items[k]);
            }
        }
        return values;
    }

    this.each = function (fn) {
        for (var k in this.items) {
            if (this.hasItem(k)) {
                fn(k, this.items[k]);
            }
        }
    }

    this.clear = function () {
        this.items = {}
        this.length = 0;
    }
}
var QueryString = function () {
    // This function is anonymous, is executed immediately and 
    // the return value is assigned to QueryString!
    var query_string = {};
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        // If first entry with this name
        if (typeof query_string[pair[0]] === "undefined") {
            query_string[pair[0]] = decodeURIComponent(pair[1]);
            // If second entry with this name
        } else if (typeof query_string[pair[0]] === "string") {
            var arr = [query_string[pair[0]], decodeURIComponent(pair[1])];
            query_string[pair[0]] = arr;
            // If third or later entry with this name
        } else {
            query_string[pair[0]].push(decodeURIComponent(pair[1]));
        }
    }
    return query_string;
}();



var parameter_table = new HashTable();
function UpdateUrlParameters() {
    var param_string = "?";
    var paramcount = parameter_table.keys().length;
    if (paramcount > 0) {
        var i = 0;
        for (var key in parameter_table.items) {
            if (parameter_table.hasItem(key) && parameter_table.items[key] !== "undefined") {
                if (i == paramcount - 1)
                    param_string += key + "=" + parameter_table.items[key];
                else
                    param_string += key + "=" + parameter_table.items[key] + "&";
            }
            i += 1;
        }
        window.history.pushState("object or string", "Title", param_string);

    }
    else {
        window.history.pushState("object or string", "Title", "/");
    }
}
function SetUrlParameter(param_name, val) {
    parameter_table.setItem(param_name, val);
    UpdateUrlParameters();
}
function ClearUrlParameters() {
    parameter_table.clear();
    UpdateUrlParameters();
}
function RemoveUrlParameter(param_name) {
    parameter_table.removeItem(param_name);
    UpdateUrlParameters();
}
 function GetUrlParameter(sParam) {
    var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : sParameterName[1];
        }
    }
};
function stringToByteArray(input) {
    var bytes = [];
    for (var i = 0; i < input.length; ++i) {
        bytes.push(input.charCodeAt(i));
    }
    return bytes;
}
function ArrayCopy(inarray, sourceindex, destinationindex, length) {
    var resault = new Array();
    var cn = 0;
    for (var i = sourceindex; i < sourceindex + length; i++) {
        resault[cn + destinationindex] = inarray[i];
        cn++;
    }

    return resault;
}
function toUTF8String(arr) {
    var i, str = '';

    for (i = 0; i < arr.length; i++) {
        str += '%' + ('0' + arr[i].toString(16)).slice(-2);
    }
    return decodeURIComponent(str);
}
function toUTF8Array(str) {
    var utf8 = [];
    for (var i = 0; i < str.length; i++) {
        var charcode = str.charCodeAt(i);
        if (charcode < 0x80) utf8.push(charcode);
        else if (charcode < 0x800) {
            utf8.push(0xc0 | (charcode >> 6),
                      0x80 | (charcode & 0x3f));
        }
        else if (charcode < 0xd800 || charcode >= 0xe000) {
            utf8.push(0xe0 | (charcode >> 12),
                      0x80 | ((charcode >> 6) & 0x3f),
                      0x80 | (charcode & 0x3f));
        }

        else {
            i++;
            charcode = 0x10000 + (((charcode & 0x3ff) << 10)
                      | (str.charCodeAt(i) & 0x3ff));
            utf8.push(0xf0 | (charcode >> 18),
                      0x80 | ((charcode >> 12) & 0x3f),
                      0x80 | ((charcode >> 6) & 0x3f),
                      0x80 | (charcode & 0x3f));
        }
    }
    return utf8;
}

function toBigIntegerArray(stringarray) {
    var codedmessage = new Array();
    for (var i = 0; i < stringarray.length; i++) {
        codedmessage[i] = BigInteger(stringarray[i]);
    }
    return codedmessage;
}
function toBase64(bytes) {
    return btoa(String.fromCharCode.apply(null, bytes));
}
function fromBase64(b64) {
    return atob(b64);
}
function rsa_encrypt(textarr, e, n) {
    var length = textarr.length;
    var resault = new Array();
    var temp = toBigIntegerArray(textarr);
    for (var i = 0; i < length; i++) {
        resault[i] = temp[i].modPow(e, n);
    }
    return resault;
}
function getRandomIntInclusive(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}
function createrandompass(length) {
    var final = "";
    for (var i = 0; i < length; i++) {
        final += String.fromCharCode(getRandomIntInclusive(33, 125));
    }
    final = final.replace("\\", "^").replace("/", "@").replace("\"", "_").replace("'", " !");
    if (final.length != length)
        return createrandompass(length);
    else
        return final;
}

var aes_key_string;
var aes_iv_string;
function beginAuthentication() {
    if (use_encryption == "True") {

        if (aes_decryption_token_received == 'False') {
            aes_key_string = createrandompass(16);//(128 / 8)
            aes_iv_string = createrandompass(16);//////////
            $.jStorage.set("aes_key_string", aes_key_string);
            $.jStorage.set("aes_iv_string", aes_iv_string);

            var e = BigInteger(rsa_e);
            var n = BigInteger(rsa_n);

            var arr = toUTF8Array(aes_key_string + " " + aes_iv_string);
            var encrtypted = rsa_encrypt(arr, e, n);
            var rsa_text = "";
            for (var i = 0; i < encrtypted.length; i++) {
                rsa_text += encrtypted[i].toString() + " ";
            }
            AjaxHelper.PostAsync("/api/apply_decryption_token", { key: toBase64(stringToByteArray(rsa_text)) }, function (response) {
                if (response == "Bad token")
                    beginAuthentication();
                else if (response != "ok") {
                    alert(response);
                    location.reload();
                }
                else {
                    aes_decryption_token_received = 'True';
                    beginAuthentication();
                }
            });
        }
        else {
            var aes_key_string = $.jStorage.get("aes_key_string");
            var aes_iv_string = $.jStorage.get("aes_iv_string");
        }

    }
    else {

    }
    continue_after_auth();
}



function encrypt(message) {
    var encrypted = CryptoJS.AES.encrypt(message, CryptoJS.enc.Utf8.parse($.jStorage.get("aes_key_string")), { keySize: 128 / 8, iv: CryptoJS.enc.Utf8.parse($.jStorage.get("aes_iv_string")), padding: CryptoJS.pad.Pkcs7, mode: CryptoJS.mode.CBC });
    return encrypted;
}

function decrypt(message) {
    var decrypted = CryptoJS.AES.decrypt(message, CryptoJS.enc.Utf8.parse($.jStorage.get("aes_key_string")), { keySize: 128 / 8, iv: CryptoJS.enc.Utf8.parse($.jStorage.get("aes_iv_string")), padding: CryptoJS.pad.Pkcs7, mode: CryptoJS.mode.CBC });
    return decrypted.toString(CryptoJS.enc.Utf8);
}
function getbetween(txt, srt, end) {
    return txt.substring(txt.lastIndexOf(srt) + srt.length, txt.lastIndexOf(end));
}

var iframe;

function navigate_base(page_path, args) {
    if (use_encryption == "True") {
        if (aes_decryption_token_received == 'True') {
            if (page_path.trim() == "" || page_path.trim() == "/")
                page_path = "?";
            AjaxHelper.PostAsync("/api/page_provider", { path: encrypt(page_path) }, function (d) {
               
                var raw_page = decrypt(d);

                var header = getbetween(raw_page, "<head>", "</head>");
                $("head").html(header);

                iframe.contentWindow.document.open();
                iframe.contentWindow.document.write(raw_page);
                iframe.contentWindow.document.close();
                $(iframe).contents().find(document).trigger("ready");
                if (page_path != "?")
                    SetUrlParameter("wpg", page_path);
                else
                    RemoveUrlParameter("wpg");

                if (args !== undefined) {
                    for (var i = 0; i < args.length;i++)
                    {
                        SetUrlParameter(args[i].name, args[i].value);
                    }
                }

                $(iframe).contents().find(document).trigger("ready");
            });
        }
    }
}
function navigateAbs(page_path) {
 
    navigate_base(page_path);
}
function navigate(page_path, params) {
    ClearUrlParameters();
    navigate_base(page_path, params);
}
function continue_after_auth() {
    if (GetUrlParameter("wpg") === undefined) {
        navigateAbs("/");
    }
    else
        navigateAbs(GetUrlParameter("wpg"));
}
window.onpopstate = function (event) {
    continue_after_auth();
};
window.onload = function () {
    iframe = document.createElement('iframe');
    document.body.appendChild(iframe);
    $(iframe).css("position", "absolute");
    $(iframe).css("top", "0");
    $(iframe).css("left", "0");
    $(iframe).css("width", "100%");
    $(iframe).css("height", "100%");
    $(iframe).attr("frameborder", "0");

    for (key in QueryString) {
        parameter_table.setItem(key, QueryString[key]);
    }

    beginAuthentication();
};
window.onunload = function () { };