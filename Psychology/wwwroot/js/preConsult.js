(function () {
    const form = document.getElementById('preConsultForm');
    const prefix = document.getElementById('prefixSelect');
    const phone = document.querySelector('.phone-input');
    const PrefixWithPhone = document.getElementById('comparePrefixWithPhone');
    const msgPref = document.querySelector('[data-valmsg-for="PhonePrefixId"]');
    const msgPhone = document.querySelector('[data-valmsg-for="PhoneNumber"]');
    const email = document.querySelector('[data-valmsg-for="Email"]');
    const closeBtn = document.querySelector('#preConsultationModal .btn-close');

    let prefixData;
    const loadPrefix = async () => {
        try {
            const res = await fetch('/Home/Prefix', { method: 'GET' });
            if (!res.ok) throw new Error('Server error: ' + res.status);
            prefixData = await res.json();

            fillPrefixOption(prefixData.prefixMetaById);
        } catch (err) {
            console.error(err);
        }
    };

    $('#preConsultationModal').on('shown.bs.modal', function () {
        loadPrefix();
    });

    // ---- Select2
    $('.select2').select2({
        theme: 'bootstrap-5',
        width: 'resolve',
        dir: 'rtl',
        dropdownParent: $('#preConsultationModal'),
        minimumResultsForSearch: 6,
        placeholder: 'کشور'
    });

    // helper: meta for selected prefix
    function currentMeta() {
        const id = prefix.value;
        if (!id) return null;

        if (prefixData.prefixMetaById && prefixData.prefixMetaById[id]) {
            const m = prefixData.prefixMetaById[id];
            // support either camelCase or PascalCase just in case
            return { expectedLength: m.expectedLength ?? m.ExpectedLength, prefix: m.prefix ?? m.Prefix };
        }
        const opt = prefix.selectedOptions[0];
        if (!opt) return null;
        const exp = opt.getAttribute('data-expected');
        const pfx = opt.getAttribute('data-prefix');
        return exp ? { expectedLength: parseInt(exp, 10), prefix: pfx || '' } : null;
    }

    function fillPrefixOption(prefixMeta) {
        Object.values(prefixMeta).forEach(p => {
            const opt = document.createElement('option');
            opt.value = p.id;
            opt.textContent = `${p.countryName} (${p.prefix})`;
            prefix.appendChild(opt);
        });
    }

    // keep numeric only (no error messages while typing)
    phone.addEventListener('input', () => {
        const onlyDigits = phone.value.replace(/\D/g, '');
        if (phone.value !== onlyDigits) phone.value = onlyDigits;
    });

    // update maxlength/placeholder on prefix change (no errors yet)
    prefix.addEventListener('change', () => {
        const meta = currentMeta();
        if (meta && meta.expectedLength > 0) {
            phone.setAttribute('maxlength', meta.expectedLength);
            phone.setAttribute('inputmode', 'numeric');
            phone.placeholder = `مثلاً 9121234567 (بدون صفر، ${toFarsiDigits(String(meta.expectedLength))} رقم)`;
        } else {
            phone.removeAttribute('maxlength');
            phone.placeholder = 'مثلاً 9121234567 (بدون صفر)';
        }
        // do NOT validate here
        clearError(msgPref);
        clearError(msgPhone);
    });

    function toFarsiDigits(str) {
        const map = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
        return str.replace(/\d/g, d => map[d]);
    }

    function setError(el, text) {
        if (!el) return;
        el.textContent = text || '';
        if (text) phone.classList.add('is-invalid'); else phone.classList.remove('is-invalid');
    }
    function clearError(el) {
        if (!el) return;
        el.textContent = '';
        phone.classList.remove('is-invalid');
    }

    // validate ONLY when submitting
    function validateOnSubmit() {
        const meta = currentMeta();
        let ok = true;

        clearError(msgPref);
        clearError(msgPhone);

        // prefix required
        if (!prefix.value) {
            setError(msgPref, 'انتخاب پیش‌شماره الزامی است.');
            ok = false;
        }

        // phone required + numeric
        if (!phone.value) {
            setError(msgPhone, 'شماره تماس الزامی است.');
            ok = false;
        } else if (!/^\d+$/.test(phone.value)) {
            setError(msgPhone, 'شماره باید فقط عدد باشد.');
            ok = false;
        } else if (meta && meta.expectedLength > 0 && phone.value.length !== meta.expectedLength) {
            const needFa = toFarsiDigits(String(meta.expectedLength));
            const hasFa = toFarsiDigits(String(phone.value.length));
            setError(PrefixWithPhone, `تعداد ارقام صحیح نیست. نیاز: ${needFa} رقم، فعلی: ${hasFa} رقم.`);
            ok = false;
        }

        return ok;
    }

    form.addEventListener('submit', (e) => {
        e.preventDefault();           // stop jQuery/unobtrusive
        if (validateOnSubmit()) {
            // native submit (bypasses jQuery handlers)
            HTMLFormElement.prototype.submit.call(form);
        }

    });

    if (closeBtn) {
        closeBtn.addEventListener('click', function () {
            if (form) {
                form.reset();
            }

            clearError(comparePrefixWithPhone);
            clearError(email);
            
            prefix.value = '';
            // notify Select2 to refresh its UI
            const event = new Event('change', { bubbles: true });
            prefix.dispatchEvent(event);
        });
    }
})();