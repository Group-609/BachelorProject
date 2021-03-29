function insertData(message)
{
  window.alert("Inserting data into database: " + message); 
    
  const url = "https://coopgame.herokuapp.com/app.js";
  const data = {say: "sent", to: message}
  $.post(url,data, function(data, status){
    console.log("${data} and status is ${status}")
  }); 
}

function addToGlobalScope()
{
  window.insertData = insertData;
  window.alert("Added insertData to global scope"); 
}

