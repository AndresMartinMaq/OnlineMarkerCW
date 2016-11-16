//init the highlither for the html syntax
hljs.initHighlightingOnLoad();

/*bind and init the event listeerns for tab clicking and chanching the corresponding views to swtich between the source code and the rendered restult for the html*/
(function ($) {
    "use strict";
    //create a module for tab switching
    var myWorkscript = {

        main_fun: function () {
            //show and hide is used and binded to the approporiate tab links, as toggle would make the same link to toggle the views and not to keep the display state.
            $("#src_code_button").bind("click", function () {
              $(".view-container-both").hide();
              $(".view-container-src").show();
              $(".view-container-render").hide();
              $( '.frame' ).attr( 'srcdoc', ' ');
              });

            $("#html_render_button").bind("click", function () {
              $(".view-container-both").hide();
              $(".view-container-src").hide();
              $( '.frame' ).attr( 'srcdoc', function () { return $( this ).attr('data-srcdoc'); });
              $(".view-container-render").show();
            });

            $("#sidebyside_button").bind("click", function () {
                $(".view-container-render").hide();
                $(".view-container-src").hide();
                $('.frame').attr('srcdoc', function () { return $(this).attr('data-srcdoc'); });
                $(".view-container-both").show();
            });

            $("input[type*=submit].red").bind("click", function () {
             if(confirm("Do you want to delete this ?"))
               document.forms[0].submit();
             else
               return false;

           }); 
        },

          //init the the event binding to the links
            init: function () {
              myWorkscript.main_fun();
              $(".view-container-both").hide();
            }
        }
        //launch the app
            $(document).ready(function () {
                myWorkscript.init();
            });

}(jQuery));