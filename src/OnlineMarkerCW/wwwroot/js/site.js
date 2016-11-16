// Write your Javascript code.
(function ($) {
    "use strict";
    //create a module for initing the sidebar collapse
    var mainApp = {

        main_fun: function () {
            //load the right menu bar in right shape for the right dimmensions
            $(window).bind("load resize", function () {
              //FIXME: Rihards - test if 768 threshhold is working ok.
                //scrollbar has to be taken into account, as css and js intepreters see windows width differently
                var scrollbarWidth=(window.innerWidth-$(window).width());
                if ($(this).width() < (768-scrollbarWidth)) {
                    $('div.sidebar-collapse').addClass('collapse')
                    $('nav img').addClass('collapse')
                } else {
                    $('div.sidebar-collapse').removeClass('collapse')
                    $('nav img').removeClass('collapse')
                }
             //resize the video frame so that it's ratio is preserved
             //$(".video-frame").css( 'height', function () { return $(this).width() / 1.7; });
            });

        },
          //init the the event binding to the window
            init: function () {
              mainApp.main_fun();

            }
        }
        //launch the app
            $(document).ready(function () {
                mainApp.init();
            });

}(jQuery));
