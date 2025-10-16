/* BEGIN EXTERNAL SOURCE */


                        document.addEventListener("DOMContentLoaded", function () {
                        const form = document.querySelector("form");
                        const usernameInput = document.getElementById("username");
                        const passwordInput = document.getElementById("password");

                        form.addEventListener("submit", function (e) {
                            const username = usernameInput.value.trim();
                            const password = passwordInput.value.trim();

                            // Check if fields are filled
                            if (!username || !password) {
                                alert("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                                e.preventDefault();
                                return;
                            }

                            // Check max length
                            if (username.length > 20) {
                                alert("Tên đăng nhập không được vượt quá 20 ký tự.");
                                e.preventDefault();
                                return;
                            }

                            if (password.length > 20) {
                                alert("Mật khẩu không được vượt quá 20 ký tự.");
                                e.preventDefault();
                                return;
                            }
                        });
                        });
                
/* END EXTERNAL SOURCE */
/* BEGIN EXTERNAL SOURCE */

document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");

    form.addEventListener("submit", function (e) {
        e.preventDefault(); // Prevent default submission

        const username = document.getElementById("username").value.trim();
        const password = document.getElementById("password").value.trim();

        if (!username || !password) {
            alert("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
            return;
        }

        if (username.length > 20 || password.length > 20) {
            alert("Tên đăng nhập và mật khẩu không được vượt quá 20 ký tự.");
            return;
        }
    });
});

/* END EXTERNAL SOURCE */
