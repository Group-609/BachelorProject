var condition = "";
function insertData(message)
{    
  const url = "https://coopgame.herokuapp.com/app.js";
  const data = {say: "sent", to: message}
  $.post(url,data, function(data, status){
    console.log("${data} and status is ${status}")
  }); 
}
function getCondition()
{
  /*
  $.ajax({
    url:"https://coopgame.herokuapp.com/app.js",
    dataType: 'jsonp', // Notice! JSONP <-- P (lowercase)
    success:function(json){
        alert("Success: " + json.condition);
    },
    error:function(){
        alert("Error");
    }      
  });
  */
  
  const myurl = "https://coopgame.herokuapp.com/app.js";  
  $.ajax({
    method: 'GET',
    url: myurl + "?callback=condition",
    dataType: 'jsonp', //we use jsonp to hack around CORS limitations
    success: function(data) {
      console.log('success');
      console.log(JSON.stringify(data));
      alert("received object: " + JSON.stringify(object));
    },
    error: function (jqXHR, exception) {
      var msg = '';
      if (jqXHR.status === 0) {
          msg = 'Not connect.\n Verify Network.';
      } else if (jqXHR.status == 404) {
          msg = 'Requested page not found. [404]';
      } else if (jqXHR.status == 500) {
          msg = 'Internal Server Error [500].';
      } else if (exception === 'parsererror') {
          msg = 'Requested JSON parse failed.';
      } else if (exception === 'timeout') {
          msg = 'Time out error.';
      } else if (exception === 'abort') {
          msg = 'Ajax request aborted.';
      } else {
          msg = 'Uncaught Error.\n' + jqXHR.responseText;
      }
      console.log(msg);
  }, 
  })
  /*
  $.get(url, function( data ) {
    alert( "Going to load the following condition:." + $(".result"));
  }, "json");
  
  $.getJSON("demo_ajax_json.js", function(result){
    $.each(result, function(i, field){
      alert( "Going to load the following condition:." + field);
    });
  });
  */
}
