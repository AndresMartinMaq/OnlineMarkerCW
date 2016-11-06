// Write your Javascript code.
(function ($) {
    "use strict";
    //create a module for initing the sidebar collapse
    var mainApp = {

        main_fun: function () {
            //load the right menu bar in right shape for the right dimmensions
            $(window).bind("load resize", function () {
              //FIXME: Rihards - test if 768 threshhold is working ok.
                if ($(this).width() < 768) {
                    $('div.sidebar-collapse').addClass('collapse')
                    $('nav img').addClass('collapse')
                } else {
                    $('div.sidebar-collapse').removeClass('collapse')
                    $('nav img').removeClass('collapse')
                }
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
