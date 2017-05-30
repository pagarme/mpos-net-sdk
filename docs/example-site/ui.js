function getById (id) {
  return document.getElementById(id);
}

function showMessage (message) {
  let messages = getById('messages').innerHTML;
  messages = '<div><pre>' + message + '</pre></div>' + messages;

  getById('messages').innerHTML = messages;
};

function getLocal (name) {
  var value = window.localStorage[name];

  if (!value) return null;

  return value;
};

function setLocal (name, value) {
  window.localStorage[name] = value;
};

function getSelected (select) {
  return select.options[select.selectedIndex].value;
};