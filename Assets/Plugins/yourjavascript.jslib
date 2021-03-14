mergeInto(LibraryManager.library, {
	ShowMessage: function (message) {
		window.insertData(Pointer_stringify(message));
	},  
});