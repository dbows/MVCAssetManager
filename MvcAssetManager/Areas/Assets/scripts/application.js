/*
* jQuery File Upload Plugin JS Example 5.0.2
* https://github.com/blueimp/jQuery-File-Upload
*
* Copyright 2010, Sebastian Tschan
* https://blueimp.net
*
* Licensed under the MIT license:
* http://creativecommons.org/licenses/MIT/
*/

/*jslint nomen: true */
/*global $ */

$(function ()
{
    'use strict';
    var getUrlVars = function ()
    {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++)
        {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    };


    // Initialize the jQuery File Upload widget:
    $('#fileupload').fileupload();
    $('#fileupload').fileupload('option', {
        maxFileSize: 10000000
    });
    var data = null;
    var _prefix = $("#prefix").val();
    var _id = $("#prefixid").val();
    var _doctype = $("#doctype").val();


    if (typeof (_prefix) != "undefined" && _prefix != "")
    {
        data = { prefix: _prefix, prefixid: _id };
        if (typeof (_doctype) != "undefined" && _doctype != "")
        {
            data["doctype"] = _doctype;
            if (_doctype == "image")
            {
                $('#fileupload').fileupload('option', {
                    acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i
                });
            } else if (_doctype == "doc")
            {
                $('#fileupload').fileupload('option', {
                    acceptFileTypes: /(\.|\/)(xlsx?|pptx?|pdf|docx?|txt|csv|vsd|psd|ai)$/i
                });
            } else if (_doctype == "media")
            {
                $('#fileupload').fileupload('option', {
                    acceptFileTypes: /(\.|\/)(aac|mp3|flac|mov|mpeg4|m4p)$/i,
                    maxFileSize: 2000000000
                });
            }
        }
    }

    // Load existing files:
    $.getJSON($('#fileupload form').prop('action'), data, function (retdata)
    {

        var files = retdata.Files;
        var assettypes = retdata.AssetTypes;
        var fu = $('#fileupload').data('fileupload');
        if (typeof (_doctype) != "undefined" && _doctype != "")
        {
            fu.option.acceptFileTypes = retdata.AssetTypes[_doctype];
        }
        fu._adjustMaxNumberOfFiles(-files.length);
        fu._renderDownload(files)
            .appendTo($('#fileupload .files'))
            .fadeIn(function ()
            {
                // Fix for IE7 and lower:
                $(this).show();
            });
    });

    // Open download dialogs via iframes,
    // to prevent aborting current uploads:
    $('#fileupload .files a:not([target^=_blank])').live('click', function (e)
    {
        e.preventDefault();
        $('<iframe style="display:none;"></iframe>')
            .prop('src', this.href)
            .appendTo('body');
    });



});