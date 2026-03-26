mergeInto(LibraryManager.library, { 
    RecognissimoUtils_Filesystem_Commit: function (callback) {
        FS.syncfs(false, () => {{{ makeDynCall('v', 'callback') }}}());
    } 
});