function deletePanel(uToken, name) {
    const uri = "http://127.0.0.1/api/user/" + uToken;

    if (!confirm('Are you sure you want to delete the panel "'+ name +'"?')) {
        return
    }

    fetch(uri,
    {
        method: 'delete'
    }).then(() => {
        window.location.reload();
    }).catch(err => {
        console.error(err)
    });
}
