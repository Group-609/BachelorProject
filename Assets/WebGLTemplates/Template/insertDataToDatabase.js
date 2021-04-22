var condition = "not set";

window.document.addEventListener('returnPlayerIdentifierToIframe', sendPlayerIdentifierToUnity, false)

function insertData(message)
{    
  const url = "https://coopgame.herokuapp.com/app.js";
  const data = {say: "sent", to: message}
  $.post(url,data, function(data, status){
    console.log("${data} and status is ${status}")
  }); 
}
function getPlayerIdentifier(){
  var event = new CustomEvent('getPlayerIdentifierFromWebsite');
  window.parent.document.dispatchEvent(event);
}

function sendPlayerIdentifierToUnity(e){
  unityInstance.SendMessage('ConditionSetter', 'GetPlayerIdentifier', e.playerIdentifier);
}

function getCondition(callback)
{
  const myurl = "https://coopgame.herokuapp.com/app.js";  
  $.ajax({
    method: 'GET',
    url: myurl + "?callback=?",
    dataType: 'jsonp', //we use jsonp to hack around CORS limitations
    success: function(data) {
      console.log('success');
      console.log(data.condition);
      condition = data.condition;
      callback(condition);
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
      callback("Failed to receive condition from server!");
  }, 
  })
}
function showSecondForm(gatheredData)
{
  var event = new CustomEvent('showSecondForm', { detail: gatheredData })
  window.parent.document.dispatchEvent(event)
}

function showLastForm(gatheredData)
{
  var event = new CustomEvent('showLastForm', { detail: gatheredData })
  window.parent.document.dispatchEvent(event)
}
