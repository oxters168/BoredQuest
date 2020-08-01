//Source: https://forum.unity.com/threads/get-the-website-url-which-is-displaying-the-unity-webgl-component.438179/

mergeInto(LibraryManager.library, {

    GetURLFromPage: function () {
        var returnStr = window.top.location.href;
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    }

    GetQueryParam: function(paramId) {
        var urlParams = new URLSearchParams(location.search);
        var param = urlParams.get(Pointer_stringify(paramId));
        console.log("JavaScript read param: " + param);
        var bufferSize = lengthBytesUTF8(param) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(param, buffer, bufferSize);
        return buffer;
    }
});