mergeInto(LibraryManager.library, {
	Save: function (message) {
		insertData(Pointer_stringify(message));
	},

	FirstConditionFinished: function () {
		showSecondForm();
	}, 
});