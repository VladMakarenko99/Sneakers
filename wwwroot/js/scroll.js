const loadButton = document.getElementById("load-more");
    if(loadButton !== null){
        loadButton.addEventListener('click', () => {
        let scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        localStorage.setItem('scrollPosition', scrollTop);
        console.log(scrollTop);
    });
    }

    const topUp = document.querySelector('.top-up');
    topUp.addEventListener('click', () => {
        window.scrollTo(0, 0);
    })