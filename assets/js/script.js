const body = document.body
// Event Listeners
const btnTheme = document.querySelector('.fa-moon')
const btnHamburger = document.querySelector('.fa-bars')
// function to add theme class
const addThemeClass = (bodyClass, btnClass) => {
	body.classList.add(bodyClass)
	btnTheme.classList.add(btnClass)
}
// add event listener to theme button
const getBodyTheme = localStorage.getItem('portfolio-theme')
const getBtnTheme = localStorage.getItem('portfolio-btn-theme')
// add event listener to theme button
addThemeClass(getBodyTheme, getBtnTheme)
//	Scroll to top
const isDark = () => body.classList.contains('dark')
// function to set theme 
const setTheme = (bodyClass, btnClass) => {
	body.classList.remove(localStorage.getItem('portfolio-theme'))
	btnTheme.classList.remove(localStorage.getItem('portfolio-btn-theme'))
	addThemeClass(bodyClass, btnClass)
	localStorage.setItem('portfolio-theme', bodyClass)
	localStorage.setItem('portfolio-btn-theme', btnClass)
}
// Event Listeners theme toggle
const toggleTheme = () =>
	isDark() ? setTheme('light', 'fa-moon') : setTheme('dark', 'fa-sun')
btnTheme.addEventListener('click', toggleTheme)
// Event Listeners hamburger menu
const displayList = () => {
	const navUl = document.querySelector('.nav__list')
	if (btnHamburger.classList.contains('fa-bars')) {
		btnHamburger.classList.remove('fa-bars')
		btnHamburger.classList.add('fa-times')
		navUl.classList.add('display-nav-list')
	} else {
		btnHamburger.classList.remove('fa-times')
		btnHamburger.classList.add('fa-bars')
		navUl.classList.remove('display-nav-list')
	}
}
btnHamburger.addEventListener('click', displayList)
// Scroll to top
const scrollUp = () => {
	const btnScrollTop = document.querySelector('.scroll-top')
	// Show scroll top button
	if (
		body.scrollTop > 500 ||
		document.documentElement.scrollTop > 500
	) {
		btnScrollTop.style.display = 'block'
	} else {
		btnScrollTop.style.display = 'none'
	}
}
document.addEventListener('scroll', scrollUp)
