mergeInto(LibraryManager.library, {
	ShowMessage: function (message) {
		insertData(Pointer_stringify(message));
	},  
});