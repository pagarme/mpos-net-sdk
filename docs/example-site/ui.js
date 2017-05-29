function getById(id) {
  return document.getElementById(id);
}

function showMessage(message) {
  let messages = getById("messages").innerHTML;
  messages = "<div><pre>" + message + "</pre></div>" + messages;

  getById("messages").innerHTML = messages;
};